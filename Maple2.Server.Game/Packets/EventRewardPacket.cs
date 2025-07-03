using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class EventRewardPacket {
    private enum Command : byte {
        Unknown1 = 1,
        ClaimTimeRunFinalReward = 3,
        ClaimTimeRunStepReward = 4,
        FlipGalleryCard = 7,
        ClaimGalleryReward = 9,
        OpenBingo = 20,
        UpdateBingo = 21,
        ClaimBingoReward = 23,
    }

    public static ByteWriter Unknown1(List<RewardItem> items) {
        var pWriter = Packet.Of(SendOp.EventReward);
        pWriter.Write<Command>(Command.Unknown1);
        pWriter.WriteInt(items.Count);
        foreach (RewardItem item in items) {
            pWriter.WriteInt(item.ItemId);
            pWriter.WriteShort(item.Rarity);
        }

        return pWriter;
    }

    public static ByteWriter ClaimTimeRunFinalReward(RewardItem rewardItem) {
        var pWriter = Packet.Of(SendOp.EventReward);
        pWriter.Write<Command>(Command.ClaimTimeRunFinalReward);
        pWriter.WriteInt(rewardItem.ItemId);
        pWriter.WriteShort(rewardItem.Rarity);

        return pWriter;
    }

    public static ByteWriter ClaimTimeRunStepReward(RewardItem rewardItem) {
        var pWriter = Packet.Of(SendOp.EventReward);
        pWriter.Write<Command>(Command.ClaimTimeRunStepReward);
        pWriter.WriteInt(rewardItem.ItemId);
        pWriter.WriteShort(rewardItem.Rarity);

        return pWriter;
    }

    public static ByteWriter FlipGalleryCard(byte index) {
        var pWriter = Packet.Of(SendOp.EventReward);
        pWriter.Write<Command>(Command.FlipGalleryCard);
        pWriter.WriteByte(index);

        return pWriter;
    }

    public static ByteWriter ClaimGalleryReward(RewardItem[] rewardItems) {
        var pWriter = Packet.Of(SendOp.EventReward);
        pWriter.Write<Command>(Command.ClaimGalleryReward);
        pWriter.WriteInt(rewardItems.Length);
        foreach (RewardItem rewardItem in rewardItems) {
            pWriter.WriteInt(rewardItem.ItemId);
            pWriter.WriteShort(rewardItem.Rarity);
        }

        return pWriter;
    }

    public static ByteWriter OpenBingo(int uid, string checkedNumbers, string rewardsClaimed, int[] bingoNumbers) {
        var pWriter = Packet.Of(SendOp.EventReward);
        pWriter.Write<Command>(Command.OpenBingo);
        pWriter.WriteInt(uid);
        pWriter.WriteUnicodeString(checkedNumbers);
        pWriter.WriteUnicodeString(rewardsClaimed);
        pWriter.WriteInt(bingoNumbers.Length);
        foreach (int number in bingoNumbers) {
            pWriter.WriteInt(number);
        }

        return pWriter;
    }

    public static ByteWriter UpdateBingo(string checkedNumbers, string rewardsClaimed) {
        var pWriter = Packet.Of(SendOp.EventReward);
        pWriter.Write<Command>(Command.UpdateBingo);
        pWriter.WriteUnicodeString(checkedNumbers);
        pWriter.WriteUnicodeString(rewardsClaimed);

        return pWriter;
    }

    public static ByteWriter ClaimBingoReward(RewardItem[] rewards) {
        var pWriter = Packet.Of(SendOp.EventReward);
        pWriter.Write<Command>(Command.ClaimBingoReward);
        pWriter.WriteInt(rewards.Length);
        foreach (RewardItem reward in rewards) {
            pWriter.WriteInt(reward.ItemId);
            pWriter.WriteShort(reward.Rarity);
        }

        return pWriter;
    }
}
