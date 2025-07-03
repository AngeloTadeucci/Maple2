using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class BonusGamePacket {
    private enum Command : byte {
        Load = 1,
        Spin = 2,
    }

    public static ByteWriter Load(IList<ItemComponent> items) {
        var pWriter = Packet.Of(SendOp.BonusGame);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteByte();
        pWriter.WriteInt(items.Count);
        foreach (ItemComponent item in items) {
            pWriter.WriteInt(item.ItemId);
            pWriter.WriteByte((byte) item.Rarity);
            pWriter.WriteInt(item.Amount);
        }
        pWriter.WriteInt();

        return pWriter;
    }

    public static ByteWriter Spin(IList<KeyValuePair<Item, int>> items) {
        var pWriter = Packet.Of(SendOp.BonusGame);
        pWriter.Write<Command>(Command.Spin);
        pWriter.WriteInt(items.Count);
        foreach (KeyValuePair<Item, int> item in items) {
            pWriter.WriteInt(item.Value);
            pWriter.WriteInt(item.Key.Id);
            pWriter.WriteInt(item.Key.Amount);
            pWriter.WriteShort((short) item.Key.Rarity);
        }

        return pWriter;
    }
}
