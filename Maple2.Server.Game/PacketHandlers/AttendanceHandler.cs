using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Event;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class AttendanceHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.Attendance;

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required ItemMetadataStorage ItemMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    private enum Command : byte {
        Claim = 0,
        BeginTimer = 1,
        Unknown2 = 2,
        Unknown3 = 3,
        EarlyParticipation = 4,
        Unknown8 = 8,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Claim:
                HandleClaim(session);
                return;
            case Command.BeginTimer:
                HandleBeginTimer(session);
                return;
            case Command.EarlyParticipation:
                HandleEarlyParticipation(session, packet);
                return;
        }
    }

    private void HandleClaim(GameSession session) {
        IList<GameEvent> gameEvents = session.FindEvent(GameEventType.AttendGift);
        if (gameEvents.Count == 0) {
            return;
        }

        foreach (GameEvent gameEvent in gameEvents) {
            if (gameEvent?.Metadata.Data is not AttendGift attendGift) {
                continue;
            }

            // Verify that the player meets the time requirements of event
            long accumulatedTimeValue = session.GameEvent.Get(GameEventUserValueType.AttendanceAccumulatedTime, gameEvent.Id, DateTime.Now.AddDays(1).Date.ToEpochSeconds()).Long();
            if ((DateTime.Now.AddSeconds(accumulatedTimeValue) - session.Player.Value.Character.LastModified).TotalSeconds < attendGift.RequiredPlaySeconds) {
                return;
            }

            DateTime completeTime = session.GameEvent.Get(GameEventUserValueType.AttendanceCompletedTimestamp, gameEvent.Id, gameEvent.EndTime).Long().FromEpochSeconds();
            if (DateTime.Now < completeTime || completeTime.Date == DateTime.Now.Date) {
                session.Send(AttendancePacket.Error(AttendanceError.s_attendGift_item_attend_already_used));
                return;
            }

            GetRewards(session, attendGift, gameEvent);
            session.GameEvent.Set(gameEvent.Id, GameEventUserValueType.AttendanceCompletedTimestamp, DateTime.Now.ToEpochSeconds());
        }
    }

    private void HandleBeginTimer(GameSession session) {
        IList<GameEvent> gameEvents = session.FindEvent(GameEventType.AttendGift);
        if (gameEvents.Count == 0) {
            return;
        }

        foreach (GameEvent gameEvent in gameEvents) {
            long expirationTime = session.GameEvent.Get(GameEventUserValueType.AttendanceAccumulatedTime, gameEvent.Id, DateTime.Now.AddDays(1).Date.ToEpochSeconds()).ExpirationTime;
            if (expirationTime < DateTime.Now.ToEpochSeconds()) {
                session.GameEvent.Set(gameEvent.Id, GameEventUserValueType.AttendanceAccumulatedTime, 0);
            }
        }
    }

    private void GetRewards(GameSession session, AttendGift attendGift, GameEvent gameEvent) {
        int rewardsClaimed = session.GameEvent.Get(GameEventUserValueType.AttendanceRewardsClaimed, gameEvent.Id, gameEvent.EndTime).Int();
        rewardsClaimed++;
        session.GameEvent.Set(gameEvent.Id, GameEventUserValueType.AttendanceRewardsClaimed, rewardsClaimed);

        RewardItem reward = attendGift.Items.ElementAtOrDefault(rewardsClaimed);
        if (default(RewardItem).Equals(reward)) {
            return;
        }

        Item? item = session.Field?.ItemDrop.CreateItem(reward.ItemId, reward.Rarity, reward.Amount);
        if (item == null) {
            return;
        }

        var receiverMail = new Mail() {
            ReceiverId = session.CharacterId,
            Type = MailType.System,
            SenderName = "MapleStory 2",
            Title = attendGift.MailTitle,
            Content = attendGift.MailContent,
        };

        using GameStorage.Request db = session.GameStorage.Context();
        receiverMail = db.CreateMail(receiverMail);
        if (receiverMail == null) {
            throw new InvalidOperationException($"Failed to create mail for attendance reward to user {session.CharacterId}");
        }

        item = db.CreateItem(receiverMail.Id, item);
        if (item == null) {
            throw new InvalidOperationException($"Failed to create reward item: {reward.ItemId}");
        }

        receiverMail.Items.Add(item);

        try {
            session.World.MailNotification(new MailNotificationRequest {
                CharacterId = session.CharacterId,
                MailId = receiverMail.Id,
            });
        } catch { /* ignored */ }
    }

    private void HandleEarlyParticipation(GameSession session, IByteReader packet) {
        int eventId = packet.ReadInt();
        // packet.ReadLong();
        GameEvent? gameEvent = session.FindEvent(eventId);
        if (gameEvent?.Metadata.Data is not AttendGift attendGift) {
            return;
        }

        // TODO: Need to interpret how this would be in the xmls. Current xml does not have this information.
        /*int skipsTotal = session.GameEventUserValue.Get(GameEventUserValueType.AttendanceEarlyParticipationRemaining, attendGift.Id, attendGift.EndTime).Int();
        if (skipsTotal >= attendGift.SkipDaysAllowed) {
            return;
        }

        switch (attendGift.SkipDayCurrencyType) {
            case AttendGiftCurrencyType.Meso:
                if (session.Currency.CanAddMeso(-attendGift.SkipDayCost) != -attendGift.SkipDayCost) {
                    session.Send(AttendancePacket.Error(AttendanceError.s_attendGift_payAttend_result_lackMoney));
                    return;
                }
                session.Currency.Meso -= attendGift.SkipDayCost;
                break;
            case AttendGiftCurrencyType.Meret:
                if (session.Currency.CanAddMeret(-attendGift.SkipDayCost) != -attendGift.SkipDayCost) {
                    session.Send(AttendancePacket.Error(AttendanceError.s_attendGift_payAttend_result_lackMerat));
                    return;
                }
                session.Currency.Meret -= attendGift.SkipDayCost;
                break;
            case AttendGiftCurrencyType.None:
            default:
                return;
        }

        session.GameEventUserValue.Set(attendGift.Id, GameEventUserValueType.AttendanceEarlyParticipationRemaining, skipsTotal + 1);
        GetRewards(session, attendGift);*/
    }
}
