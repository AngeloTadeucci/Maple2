using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class MasteryPacket {
    private enum Command : byte {
        UpdateMastery = 0,
        ClaimReward = 1,
        GetCraftedItem = 2,
        Error = 3,
    }

    public static ByteWriter UpdateMastery(MasteryType type, int mastery) {
        var pWriter = Packet.Of(SendOp.Mastery);
        pWriter.Write<Command>(Command.UpdateMastery);
        pWriter.Write<MasteryType>(type);
        pWriter.WriteInt(mastery);
        pWriter.WriteInt(); // unk

        return pWriter;
    }

    public static ByteWriter ClaimReward(int rewardBoxDetails, ICollection<ItemComponent> items) {
        var pWriter = Packet.Of(SendOp.Mastery);
        pWriter.Write<Command>(Command.ClaimReward);
        pWriter.WriteInt(rewardBoxDetails);
        pWriter.WriteInt(items.Count);
        foreach (ItemComponent item in items) {
            pWriter.WriteInt(item.ItemId);
            pWriter.WriteShort((short) item.Rarity);
        }

        return pWriter;
    }

    public static ByteWriter GetCraftedItem(MasteryType type, ICollection<ItemComponent> items) {
        var pWriter = Packet.Of(SendOp.Mastery);
        pWriter.Write<Command>(Command.GetCraftedItem);
        pWriter.WriteShort((short) type);
        pWriter.WriteInt(items.Count);
        foreach (ItemComponent item in items) {
            pWriter.WriteInt(item.ItemId);
            pWriter.WriteShort((short) item.Rarity);
        }

        return pWriter;
    }

    public static ByteWriter Error(MasteryError error) {
        var pWriter = Packet.Of(SendOp.Mastery);
        pWriter.Write<Command>(Command.Error);
        pWriter.Write<MasteryError>(error);

        return pWriter;
    }
}
