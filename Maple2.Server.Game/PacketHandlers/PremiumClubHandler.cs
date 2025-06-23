using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class PremiumClubHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.PremiumClub;

    private enum Command : byte {
        LoadClaimedItems = 1,
        ClaimItem = 2,
        LoadPackages = 3,
        PurchasePackage = 4,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required ItemMetadataStorage ItemMetadata { private get; init; }
    public required TableMetadataStorage TableMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.LoadClaimedItems:
                HandleLoadClaimedItems(session);
                return;
            case Command.ClaimItem:
                HandleClaimItem(session, packet);
                return;
            case Command.LoadPackages:
                HandleLoadPackages(session);
                return;
            case Command.PurchasePackage:
                HandlePurchasePackage(session, packet);
                break;
        }
    }

    private void HandleLoadClaimedItems(GameSession session) {
        session.Send(PremiumCubPacket.LoadItems(session.Player.Value.Account.PremiumRewardsClaimed));
    }

    private void HandleClaimItem(GameSession session, IByteReader packet) {
        int benefitId = packet.ReadInt();

        if (session.Player.Value.Account.PremiumTime < DateTimeOffset.UtcNow.ToUnixTimeSeconds()) {
            return;
        }

        if (!TableMetadata.PremiumClubTable.Items.TryGetValue(benefitId, out PremiumClubTable.Item? premiumMetadata)) {
            return;
        }

        Item? item = session.Field.ItemDrop.CreateItem(premiumMetadata.Id, premiumMetadata.Rarity, premiumMetadata.Amount);
        if (item == null || !session.Item.Inventory.CanAdd(item)) {
            return;
        }

        if (session.Player.Value.Account.PremiumRewardsClaimed.Contains(benefitId)) {
            return;
        }

        session.Player.Value.Account.PremiumRewardsClaimed.Add(benefitId);

        session.Item.Inventory.Add(item, true);
        session.Send(PremiumCubPacket.ClaimItem(benefitId));
    }

    private void HandleLoadPackages(GameSession session) {
        session.Send(PremiumCubPacket.LoadPackages());
    }

    private void HandlePurchasePackage(GameSession session, IByteReader packet) {
        int packageId = packet.ReadInt();

        if (!TableMetadata.PremiumClubTable.Packages.TryGetValue(packageId, out PremiumClubTable.Package? premiumMetadata)) {
            return;
        }

        if (premiumMetadata.Disabled) {
            return;
        }

        if ((premiumMetadata.StartDate > 0 && DateTimeOffset.UtcNow.ToUnixTimeSeconds() < premiumMetadata.StartDate) || (premiumMetadata.EndDate > 0 && DateTimeOffset.UtcNow.ToUnixTimeSeconds() > premiumMetadata.EndDate)) {
            return;
        }

        if (session.Currency.Meret < premiumMetadata.Price) {
            session.Send(NoticePacket.Notice(NoticePacket.Flags.Alert | NoticePacket.Flags.Message, StringCode.s_err_lack_merat));
            return;
        }

        session.Currency.Meret -= premiumMetadata.Price;

        foreach (PremiumClubTable.Item item in premiumMetadata.BonusItems) {
            Item? bonusItem = session.Field.ItemDrop.CreateItem(item.Id, item.Rarity, item.Amount);
            if (bonusItem == null) {
                continue;
            }

            if (!session.Item.Inventory.Add(bonusItem, true)) {
                session.Item.MailItem(bonusItem);
            }
        }

        session.Send(PremiumCubPacket.PurchasePackage(packageId));
        session.Config.UpdatePremiumTime(premiumMetadata.Period);
    }
}
