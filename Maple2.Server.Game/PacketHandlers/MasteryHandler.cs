using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.PacketHandlers;

public class MasteryHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.Mastery;

    private enum Command : byte {
        Reward = 1,
        Craft = 2,
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
            case Command.Reward:
                HandleReward(session, packet);
                break;
            case Command.Craft:
                HandleCraft(session, packet);
                break;
        }
    }

    private void HandleReward(GameSession session, IByteReader packet) {
        int rewardBoxDetails = packet.ReadInt();
        var type = (MasteryType) (rewardBoxDetails / 1000);
        int level = rewardBoxDetails % 100;

        if (session.Player.Value.Unlock.MasteryRewardsClaimed.TryGetValue(rewardBoxDetails, out bool isClaimed) && isClaimed) {
            session.Send(MasteryPacket.Error(MasteryError.s_mastery_error_unknown));
            return;
        }

        if (!TableMetadata.MasteryRewardTable.Entries.TryGetValue(type, level, out MasteryRewardTable.Entry? entry)) {
            return;
        }

        if (session.Mastery[type] < entry.Value) {
            session.Send(MasteryPacket.Error(MasteryError.s_mastery_error_invalid_level));
            return;
        }

        Item? rewardItem = session.Item.CreateItem(entry.ItemId, entry.ItemRarity, entry.ItemAmount);
        if (rewardItem == null) {
            return;
        }

        if (!session.Item.Inventory.Add(rewardItem, true)) {
            session.Send(ChatPacket.Alert(StringCode.s_err_inventory));
            return;
        }

        session.Player.Value.Unlock.MasteryRewardsClaimed.Add(rewardBoxDetails, true);
        session.Send(MasteryPacket.ClaimReward(rewardBoxDetails, new List<ItemComponent>() {
            new(entry.ItemId, (short) entry.ItemRarity, entry.ItemRarity, ItemTag.None),
        }));
    }

    private void HandleCraft(GameSession session, IByteReader packet) {
        int recipeId = packet.ReadInt();

        if (!TableMetadata.MasteryRecipeTable.Entries.TryGetValue(recipeId, out MasteryRecipeTable.Entry? entry)) {
            return;
        }

        // TODO: Check if player has completed the required quests.

        if (session.Mastery[entry.Type] < entry.RequiredMastery) {
            session.Send(MasteryPacket.Error(MasteryError.s_mastery_error_lack_mastery));
        }

        if (session.Currency.CanAddMeso(-entry.RequiredMeso) != -entry.RequiredMeso) {
            session.Send(MasteryPacket.Error(MasteryError.s_mastery_error_lack_meso));
            return;
        }

        if (!session.Item.Inventory.ConsumeItemComponents(entry.RequiredItems)) {
            session.Send(MasteryPacket.Error(MasteryError.s_mastery_error_lack_item));
            return;
        }

        session.Currency.Meso -= entry.RequiredMeso;

        if (!entry.NoRewardExp) {
            session.Mastery[entry.Type] += entry.RewardMastery;
        }

        foreach (ItemComponent rewardItem in entry.RewardItems) {
            Item? item = session.Item.CreateItem(rewardItem.ItemId, rewardItem.Rarity, rewardItem.Amount);
            if (item == null) {
                continue;
            }

            if (!session.Item.Inventory.Add(item, true)) {
                session.Item.MailItem(item);
            }
        }
        session.Exp.AddExp(ExpType.manufacturing);
        session.ConditionUpdate(ConditionType.mastery_manufacturing, codeLong: recipeId);
        session.Send(MasteryPacket.GetCraftedItem(entry.Type, (ICollection<ItemComponent>) entry.RewardItems));
    }
}
