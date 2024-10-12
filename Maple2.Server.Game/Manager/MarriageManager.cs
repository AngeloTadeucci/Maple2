using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Sync;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Util.Sync;
using Serilog;

namespace Maple2.Server.Game.Manager;

public sealed class MarriageManager {
    private readonly GameSession session;

    public Marriage Marriage;
    public WeddingHall WeddingHall;
    private readonly CancellationTokenSource tokenSource;

    public MarriagePartner Partner => session.CharacterId == Marriage.Partner1.CharacterId ? Marriage.Partner2 : Marriage.Partner1;

    public WeddingHall? UnconfirmedReservation;
    private long couponItemUid;

    private readonly ILogger logger = Log.Logger.ForContext<MarriageManager>();

    public MarriageManager(GameSession session) {
        this.session = session;
        tokenSource = new CancellationTokenSource();
        using GameStorage.Request db = session.GameStorage.Context();
        Marriage = db.GetMarriage(session.CharacterId) ?? Marriage.Default;
        Marriage.Exp = GetExp();

        WeddingHall = db.GetWeddingHall(Marriage) ?? WeddingHall.Default;
    }

    public void Load() {
        CheckEngagementExpiration();
        CheckMarriageHallExpiration();
        session.Send(WeddingPacket.UpdateMarriage(Marriage));
        session.Send(WeddingPacket.UpdateHall(WeddingHall));
    }

    private void CheckEngagementExpiration() {
        if (Marriage.CreationTime <= 0 || Marriage.Status != MaritalStatus.Engaged ||
            Marriage.CreationTime.FromEpochSeconds().AddDays(7) >= DateTime.Now) {
            return;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        if (!db.DeleteMarriage(Marriage.Id)) {
            session.Send(WeddingPacket.Error(WeddingError.s_wedding_result_err_system));
            return;
        }

        var mail = new Mail {
            ReceiverId = Marriage.Partner2.CharacterId,
            SenderId = Marriage.Partner2.CharacterId,
            Type = MailType.System,
            Content = 40000006.ToString(),
        };

        mail = db.CreateMail(mail);
        if (mail == null) {
            logger.Error("Failed to create mail for expired engagement for {CharacterId}", Marriage.Partner2.CharacterId);
        } else {
            try {
                session.World.MailNotification(new MailNotificationRequest {
                    CharacterId = Marriage.Partner2.CharacterId,
                    MailId = mail.Id,
                });
            } catch { /* ignored */
            }
        }

        var partnerMail = new Mail {
            ReceiverId = Marriage.Partner1.CharacterId,
            SenderId = Marriage.Partner1.CharacterId,
            Type = MailType.System,
            Content = 40000006.ToString(),
        };

        partnerMail = db.CreateMail(partnerMail);
        if (partnerMail == null) {
            logger.Error("Failed to create mail for expired engagement for {CharacterId}", Marriage.Partner1.CharacterId);
        } else {
            try {
                session.World.MailNotification(new MailNotificationRequest {
                    CharacterId = Marriage.Partner1.CharacterId,
                    MailId = partnerMail.Id,
                });
            } catch { /* ignored */
            }
        }


        try {
            session.World.Marriage(new MarriageRequest {
                ReceiverId = Marriage.Partner1.CharacterId,
                RemoveMarriage = new MarriageRequest.Types.RemoveMarriage(),
            });
        } catch { /* ignored */
        }

        RemoveMarriage(false);
    }

    public void CheckMarriageHallExpiration() {
        if (WeddingHall.Id == 0 || WeddingHall.CeremonyTime.FromEpochSeconds().AddHours(1) > DateTime.Now) {
            return;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        if (!db.DeleteWeddingHall(WeddingHall.Id)) {
            logger.Error("Failed to delete wedding hall for {CharacterId}. Hall Id: {WeddingHallId}", session.CharacterId, WeddingHall.Id);
            return;
        }

        try {
            session.World.Marriage(new MarriageRequest {
                ReceiverId = Partner.CharacterId,
                RemoveWeddingHall = new MarriageRequest.Types.RemoveWeddingHall(),
            });
        } catch { /* ignored */ }

        RemoveMarriage(false);
    }

    public void RemoveMarriage(bool update = true) {
        session.Marriage.Marriage = Marriage.Default;
        session.Player.Value.Character.MarriageInfo = new MarriageInfo();
        if (update) {
            session.Send(WeddingPacket.UpdateMarriage(Marriage));
        }
    }

    public void RemoveWeddingHall(bool update = true) {
        session.Marriage.WeddingHall = WeddingHall.Default;
        if (update) {
            session.Send(WeddingPacket.UpdateHall(WeddingHall));
        }
    }

    private long GetExp() {
        DateTime creationTime = DateTimeOffset.FromUnixTimeSeconds(Marriage.CreationTime).LocalDateTime;
        int daysMarried = (DateTime.Now - creationTime).Days;

        return daysMarried * 5 + Marriage.ExpHistory.Sum(exp => exp.Amount);
    }

    public WeddingError Propose(FieldPlayer target) {
        if (!session.Item.Inventory.Find(Constant.ProposalItemId).Any()) {
            return WeddingError.s_wedding_result_err_not_find_propose_item;
        }

        if (session.Marriage.Marriage.Status == MaritalStatus.Engaged) {
            return WeddingError.s_wedding_result_err_propose_state_to;
        }

        if (session.Marriage.Marriage.Status == MaritalStatus.Married) {
            return WeddingError.s_wedding_result_err_marriage_state_to;
        }


        if (!session.Buddy.IsBuddy(target.Value.Character.Id)) {
            return WeddingError.s_wedding_result_err_not_buddy;
        }

        if (!target.Session.Item.Inventory.Find(Constant.ProposalItemId).Any()) {
            return WeddingError.s_wedding_result_err_not_find_propose_item;
        }

        return WeddingError.s_wedding_msg_box_propose_to;
    }

    public WeddingError AcceptProposal(FieldPlayer target) {
        // Find the proposal items of each player
        Item? proposalItem = session.Item.Inventory.Find(Constant.ProposalItemId).FirstOrDefault();
        if (proposalItem == null) {
            return WeddingError.s_wedding_result_err_not_find_propose_item;
        }

        Item? otherProposalItem = target.Session.Item.Inventory.Find(Constant.ProposalItemId).FirstOrDefault();
        if (otherProposalItem == null) {
            return WeddingError.s_wedding_result_err_not_find_propose_item;
        }

        // Remove the proposal items from each player
        if (!session.Item.Inventory.Consume(proposalItem.Uid, 1)) {
            return WeddingError.s_wedding_result_err_not_find_propose_item;

        }

        if (!target.Session.Item.Inventory.Consume(otherProposalItem.Uid, 1)) {
            // TODO: Something is broken about this
            //return WeddingError.s_wedding_result_err_not_find_propose_item;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        Marriage? marriage = db.CreateMarriage(session.CharacterId, target.Session.CharacterId);
        if (marriage == null) {
            return WeddingError.s_wedding_result_err_system;
        }
        Marriage = marriage;
        session.Send(WeddingPacket.Engaged(Marriage));

        Marriage? targetMarriage = db.GetMarriage(target.Session.CharacterId);
        if (targetMarriage == null) {
            return WeddingError.s_wedding_result_err_system;
        }

        target.Session.Marriage.Marriage = targetMarriage;
        target.Session.Send(WeddingPacket.Engaged(targetMarriage));
        return WeddingError.none;
    }

    public bool ValidHallTime(long ceremonyTime, out WeddingError error) {
        using GameStorage.Request db = session.GameStorage.Context();
        if (ceremonyTime > 0) {
            if (!db.WeddingHallTimeIsAvailable(ceremonyTime)) {
                error = WeddingError.s_wedding_result_already_same_reservation;
                return false;
            }

            if (ceremonyTime < DateTime.Now.ToEpochSeconds()) {
                error = WeddingError.s_wedding_result_early_wedding_reserve;
                return false;
            }
        }
        error = WeddingError.none;
        return true;
    }

    public WeddingError ReserveHall(WeddingHall hall, bool useCoupon = false) {
        if (!ValidHallTime(hall.CeremonyTime, out WeddingError error)) {
            return error;
        }

        if (!session.TableMetadata.WeddingTable.Packages.TryGetValue(hall.PackageId, out WeddingPackage? package) ||
            !package.Halls.TryGetValue(hall.PackageHallId, out WeddingPackage.HallData? hallData)) {
            return WeddingError.s_wedding_result_err_invalid_wedding_hall;
        }

        if (useCoupon) {
            ItemTag couponTag = package.Id switch {
                1 => ItemTag.WeddingHallCoupon_Grade1,
                2 => ItemTag.WeddingHallCoupon_Grade2,
                3 => ItemTag.WeddingHallCoupon_Grade3,
                _ => ItemTag.None,
            };
            IList<Item> items = session.Item.Inventory.Find(couponTag).ToList();
            if (!items.Any()) {
                return WeddingError.s_wedding_result_lack_coupon;
            }

            couponItemUid = items.First().Uid;
        } else {
            if (session.Currency.CanAddMeret(-hallData.MeretCost) != -hallData.MeretCost) {
                return WeddingError.s_wedding_result_err_lack_meratal;
            }
        }

        if (!session.Field.TryGetPlayer(session.Marriage.Marriage.Partner1.Name, out FieldPlayer? partner)) {
            return WeddingError.s_wedding_result_err_same_field;
        }

        session.Marriage.UnconfirmedReservation = hall;
        partner.Session.Marriage.UnconfirmedReservation = hall;

        partner.Session.Send(WeddingPacket.Reserve(hall));
        return WeddingError.s_wedding_result_send_wedding_hall_reservation;
    }

    public WeddingError ConfirmReservation() {
        if (UnconfirmedReservation == null) {
            return WeddingError.s_wedding_result_err_invalid_wedding_hall;
        }

        if (!session.TableMetadata.WeddingTable.Packages.TryGetValue(UnconfirmedReservation.PackageId, out WeddingPackage? package) ||
            !package.Halls.TryGetValue(UnconfirmedReservation.PackageHallId, out WeddingPackage.HallData? hallData)) {
            return WeddingError.s_wedding_result_err_invalid_wedding_hall;
        }

        if (!session.Field.TryGetPlayerById(session.Marriage.Marriage.Partner1.CharacterId, out FieldPlayer? partner)) {
            return WeddingError.s_wedding_result_err_same_field;
        }

        if (!PayWeddingHall(partner, hallData, 0, out WeddingError payError)) {
            return payError;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        WeddingHall? weddingHall = db.CreateWeddingHall(UnconfirmedReservation, Marriage);
        if (weddingHall == null) {
            logger.Error("Failed to create wedding hall for {CharacterId}", session.CharacterId);
            return WeddingError.s_wedding_result_err_system;
        }

        WeddingHall = weddingHall;
        partner.Session.Marriage.WeddingHall = weddingHall;

        partner.Session.Send(WeddingPacket.ConfirmReservation(WeddingHall));
        session.Send(WeddingPacket.ConfirmReservation(WeddingHall));

        session.ConditionUpdate(ConditionType.wedding_hall_reserve);
        partner.Session.ConditionUpdate(ConditionType.wedding_hall_reserve);

        return WeddingError.s_wedding_propose_ok;
    }

    private bool PayWeddingHall(FieldPlayer partner, WeddingPackage.HallData hallData, long differenceCost, out WeddingError error) {
        if (couponItemUid != 0) {
            if (!partner.Session.Item.Inventory.Consume(couponItemUid, 1)) {
                error = WeddingError.s_wedding_result_lack_coupon;
                return false;
            }
            couponItemUid = 0;
        } else {
            long cost = differenceCost > 0 ? differenceCost : hallData.MeretCost;
            if (partner.Session.Currency.CanAddMeret(-cost) != -cost) {
                error = WeddingError.s_wedding_result_err_lack_meratal;
                return false;
            }
            partner.Session.Currency.Meret -= cost;

            // Mail gift only when paying with Meret, otherwise users can exploit.
            MailReservationGifts(partner, hallData);
        }

        error = WeddingError.none;
        return true;
    }

    private void MailReservationGifts(FieldPlayer partner, WeddingPackage.HallData metadata) {
        var mail = new Mail {
            ReceiverId = partner.Value.Character.Id,
            SenderId = partner.Value.Character.Id,
            Content = 40000008.ToString(), // Specific ID in string/systemmailcontent[locale].xml
            Type = MailType.System,
        };

        using GameStorage.Request db = session.GameStorage.Context();
        mail = db.CreateMail(mail);
        if (mail == null) {
            logger.Error("Failed to create mail for wedding hall reservation for {CharacterId}", partner.Value.Character.Id);
            return;
        }

        foreach (WeddingPackage.HallData.Item rewardItem in metadata.CompleteItems) {
            Item? item = session.Field.ItemDrop.CreateItem(rewardItem.ItemId, rewardItem.Rarity, rewardItem.Amount);
            if (item == null) {
                continue;
            }

            item = db.CreateItem(mail.Id, item);
            if (item == null) {
                logger.Error("Failed to create item for wedding hall reservation for {CharacterId}", partner.Value.Character.Id);
                continue;
            }

            mail.Items.Add(item);
        }

        try {
            session.World.MailNotification(new MailNotificationRequest {
                CharacterId = partner.Value.Character.Id,
                MailId = mail.Id,
            });
        } catch { /* ignored */
        }
    }

    public void CancelReservationRequest() {
        if (session.CharacterId != WeddingHall.ReserverCharacterId) {
            session.Send(WeddingPacket.Error(WeddingError.s_wedding_result_only_booking_character));
            return;
        }

        // If wedding is about to start, cannot cancel
        if (DateTime.Now >= WeddingHall.CeremonyTime.FromEpochSeconds().AddMinutes(-5)) {
            session.Send(WeddingPacket.Error(WeddingError.s_wedding_result_late_wedding_reserve_modify_time));
            return;
        }

        // If the wedding already started, cannot cancel
        if (DateTime.Now > WeddingHall.CeremonyTime.FromEpochSeconds()) {
            session.Send(WeddingPacket.Error(WeddingError.s_wedding_result_late_wedding_reserve_modify_time));
            return;
        }

        if (!session.Field.TryGetPlayerById(Partner.CharacterId, out FieldPlayer? fieldPartner)) {
            session.Send(WeddingPacket.Error(WeddingError.s_wedding_result_err_same_field));
            return;
        }

        session.Send(WeddingPacket.Error(WeddingError.s_wedding_result_send_wedding_hall_reservation_cancel));
        fieldPartner.Session.Send(WeddingPacket.RequestCancelReservation());
    }

    public WeddingError CancelReservationReply(WeddingError response) {
        if (!session.Field.TryGetPlayerById(Partner.CharacterId, out FieldPlayer? fieldPartner)) {
            return WeddingError.s_wedding_result_err_same_field;
        }

        if (response == WeddingError.decline_reservation_cancel) {
            session.Send(WeddingPacket.DeclineCancelReservation());
            fieldPartner.Session.Send(WeddingPacket.DeclinedCancelReservation(response));
            return WeddingError.none;
        }

        // Give coupon
        int couponItemId = WeddingHall.PackageId switch {
            1 => Constant.Grade1WeddingCouponItemId,
            2 => Constant.Grade2WeddingCouponItemId,
            3 => Constant.Grade3WeddingCouponItemId,
            _ => 0,
        };

        if (couponItemId == 0) {
            logger.Error("Failed to find coupon item for wedding hall {WeddingHallId}", WeddingHall.Id);
            return WeddingError.s_wedding_result_err_system;
        }

        // TODO: Fix rarity ?
        Item? coupon = session.Field.ItemDrop.CreateItem(couponItemId, 1, 1);
        if (coupon == null) {
            logger.Error("Failed to create wedding coupon for {CharacterId}", session.CharacterId);
            return WeddingError.s_wedding_result_err_system;
        }

        var mail = new Mail {
            ReceiverId = Partner.CharacterId,
            SenderId = Partner.CharacterId,
            Type = MailType.System,
            Content = 40000013.ToString(), // Specific ID in string/systemmailcontent[locale].xml
        };

        using GameStorage.Request db = session.GameStorage.Context();

        mail = db.CreateMail(mail);
        if (mail == null) {
            logger.Error("Failed to create mail for wedding hall cancellation for {CharacterId}", Partner.CharacterId);
            return WeddingError.s_wedding_result_err_system;
        }

        coupon = db.CreateItem(mail.Id, coupon);
        if (coupon == null) {
            logger.Error("Failed to create coupon item for wedding hall cancellation for {CharacterId}", Partner.CharacterId);
            return WeddingError.s_wedding_result_err_system;
        }

        mail.Items.Add(coupon);
        try {
            session.World.MailNotification(new MailNotificationRequest {
                CharacterId = Partner.CharacterId,
                MailId = mail.Id,
            });
        } catch { /* ignored */
        }

        if (!db.DeleteWeddingHall(WeddingHall.Id)) {
            logger.Error("Failed to delete wedding hall for {CharacterId}. Hall Id: {WeddingHallId}", session.CharacterId, WeddingHall.Id);
        }

        session.Send(WeddingPacket.AcceptCancelReservation(response));
        fieldPartner.Session.Send(WeddingPacket.AcceptCancelReservation(response));
        WeddingHall = WeddingHall.Default;
        fieldPartner.Session.Marriage.WeddingHall = WeddingHall.Default;

        session.ConditionUpdate(ConditionType.wedding_hall_cancel);
        fieldPartner.Session.ConditionUpdate(ConditionType.wedding_hall_cancel);
        return WeddingError.none;
    }

    public WeddingError CancelNoRefund() {
        if (!session.Field.TryGetPlayerById(Partner.CharacterId, out FieldPlayer? fieldPartner)) {
            return WeddingError.s_wedding_result_err_same_field;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        if (!db.DeleteWeddingHall(WeddingHall.Id)) {
            logger.Error("Failed to delete wedding hall for {CharacterId}. Hall Id: {WeddingHallId}", session.CharacterId, WeddingHall.Id);
        }

        session.Send(WeddingPacket.AcceptCancelReservation(WeddingError.accept_reservation_cancel));
        fieldPartner.Session.Send(WeddingPacket.AcceptCancelReservation(WeddingError.accept_reservation_cancel));
        WeddingHall = WeddingHall.Default;
        fieldPartner.Session.Marriage.WeddingHall = WeddingHall.Default;

        session.ConditionUpdate(ConditionType.wedding_hall_cancel);
        fieldPartner.Session.ConditionUpdate(ConditionType.wedding_hall_cancel);

        return WeddingError.none;
    }

    public WeddingError ChangeReservationRequest(WeddingHall hall) {
        if (WeddingHall.Id == 0) {
            return WeddingError.s_wedding_result_err_invalid_wedding_hall;
        }

        // If it's all the same, it's unnecessary to change
        if (hall.CeremonyTime == WeddingHall.CeremonyTime &&
            hall.PackageId == WeddingHall.PackageId &&
            hall.PackageHallId == WeddingHall.PackageHallId &&
            hall.Public == WeddingHall.Public) {
            return WeddingError.s_wedding_result_already_weddinghall_reserve;
        }

        if (!session.TableMetadata.WeddingTable.Packages.TryGetValue(hall.PackageId, out WeddingPackage? unconfirmedPackage) ||
            !unconfirmedPackage.Halls.TryGetValue(hall.PackageHallId, out WeddingPackage.HallData? unconfirmedHallData)) {
            return WeddingError.s_wedding_result_err_invalid_wedding_hall;
        }

        if (!session.TableMetadata.WeddingTable.Packages.TryGetValue(WeddingHall.PackageId, out WeddingPackage? currentWeddingPackage) ||
            !currentWeddingPackage.Halls.TryGetValue(WeddingHall.PackageHallId, out WeddingPackage.HallData? currentHallData)) {
            return WeddingError.s_wedding_result_err_invalid_wedding_hall;
        }

        // Cannot tier down
        if (unconfirmedHallData.Tier < currentHallData.Tier) {
            return WeddingError.s_wedding_result_err_invalid_wedding_hall;
        }

        long cost = GetWeddingHallChangeCost(currentHallData, unconfirmedHallData);
        if (session.Currency.CanAddMeret(-cost) != -cost) {
            return WeddingError.s_wedding_result_err_lack_meratal;
        }

        if (!ValidHallTime(hall.CeremonyTime, out WeddingError error)) {
            return error;
        }

        if (!session.Field.TryGetPlayerById(Partner.CharacterId, out FieldPlayer? partnerPlayer)) {
            return WeddingError.s_wedding_result_err_same_field;
        }

        UnconfirmedReservation = hall;
        partnerPlayer.Session.Marriage.UnconfirmedReservation = hall;

        partnerPlayer.Session.Send(WeddingPacket.ChangeReservationRequest(hall, cost));
        return WeddingError.s_wedding_result_send_wedding_hall_reservation_change;
    }

    public WeddingError ChangeReservationReply(WeddingError response) {
        if (UnconfirmedReservation == null) {
            return WeddingError.s_wedding_result_err_invalid_wedding_hall;
        }

        if (!session.TableMetadata.WeddingTable.Packages.TryGetValue(UnconfirmedReservation.PackageId, out WeddingPackage? package) ||
            !package.Halls.TryGetValue(UnconfirmedReservation.PackageHallId, out WeddingPackage.HallData? hallData)) {
            return WeddingError.s_wedding_result_err_invalid_wedding_hall;
        }

        if (!session.Field.TryGetPlayerById(session.Marriage.Marriage.Partner1.CharacterId, out FieldPlayer? partner)) {
            return WeddingError.s_wedding_result_err_same_field;
        }

        if (response == WeddingError.decline_reservation_change) {
            // TODO: Confirm these packets. There seems to be no response.
            session.Send(WeddingPacket.DeclineReservationChange());
            partner.Session.Send(WeddingPacket.DeclineReservationChange());
            return WeddingError.none;
        }

        if (!session.TableMetadata.WeddingTable.Packages.TryGetValue(WeddingHall.PackageId, out WeddingPackage? currentWeddingPackage) ||
            !currentWeddingPackage.Halls.TryGetValue(WeddingHall.PackageHallId, out WeddingPackage.HallData? currentHallData)) {
            logger.Error("Failed to find current wedding hall data for {CharacterId}", session.CharacterId);
            return WeddingError.s_wedding_result_err_system;
        }

        long differenceCost = GetWeddingHallChangeCost(currentHallData, hallData);

        if (!PayWeddingHall(partner, hallData, differenceCost, out WeddingError payError)) {
            return payError;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        if (!db.DeleteWeddingHall(WeddingHall.Id)) {
            logger.Error("Failed to delete wedding hall for {CharacterId}. Hall Id: {WeddingHallId}", session.CharacterId, WeddingHall.Id);
            return WeddingError.s_wedding_result_err_system;
        }

        WeddingHall? weddingHall = db.CreateWeddingHall(UnconfirmedReservation, Marriage);
        if (weddingHall == null) {
            logger.Error("Failed to create wedding hall for {CharacterId}", session.CharacterId);
            return WeddingError.s_wedding_result_err_system;
        }

        WeddingHall = weddingHall;
        partner.Session.Marriage.WeddingHall = weddingHall;
        session.Send(WeddingPacket.AcceptReservationChange(WeddingHall, differenceCost));
        partner.Session.Send(WeddingPacket.AcceptReservationChange(WeddingHall, differenceCost));

        return WeddingError.none;
    }

    private long GetWeddingHallChangeCost(WeddingPackage.HallData currentHallData, WeddingPackage.HallData unconfirmedHallData) {
        return Math.Max(0, unconfirmedHallData.MeretCost - currentHallData.MeretCost);
    }

    #region PlayerInfo Events
    private void BeginListen(MarriagePartner partner) {
        if (Marriage.Id == 0) {
            return;
        }
        // Clean up previous token if necessary
        if (partner.TokenSource != null) {
            logger.Warning("BeginListen called on Member {Id} that was already listening", partner.CharacterId);
            EndListen(partner);
        }

        partner.TokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokenSource.Token);
        CancellationToken token = partner.TokenSource.Token;
        var listener = new PlayerInfoListener(UpdateField.Marriage, (type, info) => SyncUpdate(token, partner.CharacterId, type, info));
        session.PlayerInfo.Listen(partner.Info!.CharacterId, listener);
    }

    private void EndListen(MarriagePartner partner) {
        partner.TokenSource?.Cancel();
        partner.TokenSource?.Dispose();
        partner.TokenSource = null;
    }

    private bool SyncUpdate(CancellationToken cancel, long id, UpdateField type, IPlayerInfo info) {
        if (cancel.IsCancellationRequested || Marriage.Id == 0) {
            return true;
        }

        MarriagePartner? partner = Marriage.Partner1.CharacterId == id ? Marriage.Partner1 :
            Marriage.Partner2.CharacterId == id ? Marriage.Partner2 : null;
        if (partner == null) {
            return true;
        }

        bool wasOnline = partner.Info!.Online;
        string name = partner.Info.Name;
        partner.Info.Update(type, info);

        if (name != partner.Info.Name || wasOnline != partner.Info.Online) {
            session.Send(WeddingPacket.UpdateMarriage(Marriage));
        }
        return false;
    }
    #endregion
}
