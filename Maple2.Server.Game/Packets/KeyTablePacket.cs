using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Manager.Config;

namespace Maple2.Server.Game.Packets;

public static class KeyTablePacket {
    private enum Command : byte {
        Load = 0,
        LoadHotBar = 7,
        Prompt = 9,
    }

    public static ByteWriter LoadDefault() {
        var pWriter = Packet.Of(SendOp.KeyTable);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteBool(true);

        return pWriter;
    }

    public static ByteWriter Load(ICollection<KeyBind> keyBinds, short activeHotBar, IReadOnlyList<HotBar> hotBars) {
        var pWriter = Packet.Of(SendOp.KeyTable);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteBool(false);
        // KeyBind
        pWriter.WriteInt(keyBinds.Count);
        foreach (KeyBind keyBind in keyBinds) {
            pWriter.Write<KeyBind>(keyBind);
        }
        // HotBar
        pWriter.WriteShort(activeHotBar);
        pWriter.WriteShort((short) hotBars.Count);
        foreach (HotBar hotBar in hotBars) {
            pWriter.WriteInt(hotBar.Slots.Length);
            for (int i = 0; i < hotBar.Slots.Length; i++) {
                pWriter.WriteInt(i);
                pWriter.Write<QuickSlot>(hotBar.Slots[i]);
            }
        }

        return pWriter;
    }

    public static ByteWriter LoadHotBar(short activeHotBar, IReadOnlyList<HotBar> hotBars) {
        var pWriter = Packet.Of(SendOp.KeyTable);
        pWriter.Write<Command>(Command.LoadHotBar);
        pWriter.WriteShort(activeHotBar);
        pWriter.WriteShort((short) hotBars.Count);
        foreach (HotBar hotBar in hotBars) {
            pWriter.WriteInt(hotBar.Slots.Length);
            for (int i = 0; i < hotBar.Slots.Length; i++) {
                pWriter.WriteInt(i);
                pWriter.Write<QuickSlot>(hotBar.Slots[i]);
            }
        }

        return pWriter;
    }

    public static ByteWriter Prompt() {
        var pWriter = Packet.Of(SendOp.KeyTable);
        pWriter.Write<Command>(Command.Prompt);

        return pWriter;
    }
}
