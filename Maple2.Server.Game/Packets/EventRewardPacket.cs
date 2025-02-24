using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class EventRewardPacket {
    private enum Command : byte {
        FlipGalleryCard = 7,
        ClaimGalleryReward = 9,
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
}
