using Maple2.Model.Game.Event;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class GameEventPacket {
    private enum Command : byte {
        Load = 0,
        Unknown1 = 1,
        Unknown2 = 2,
        Unknown3 = 3,
    }

    public static ByteWriter Load(IList<GameEvent> gameEvents) {
        var pWriter = Packet.Of(SendOp.GameEvent);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(gameEvents.Count);
        foreach (GameEvent gameEvent in gameEvents) {
            pWriter.WriteUnicodeString(gameEvent.Name);
            pWriter.WriteClass<GameEventInfo>(gameEvent.EventInfo);
        }

        return pWriter;
    }

    public static ByteWriter Unknown1(IList<GameEvent> gameEvents) {
        var pWriter = Packet.Of(SendOp.GameEvent);
        pWriter.Write<Command>(Command.Unknown1);
        pWriter.WriteInt(gameEvents.Count);
        foreach (GameEvent gameEvent in gameEvents) {
            pWriter.WriteUnicodeString(gameEvent.Name);
            pWriter.WriteClass<GameEventInfo>(gameEvent.EventInfo);
        }

        return pWriter;
    }

    public static ByteWriter Unknown2(IList<int> values) {
        var pWriter = Packet.Of(SendOp.GameEvent);
        pWriter.Write<Command>(Command.Unknown2);
        pWriter.WriteInt(values.Count);
        foreach (int value in values) {
            pWriter.WriteInt(value);
        }

        return pWriter;
    }

    public static ByteWriter Unknown3(IList<GameEvent> gameEvents) {
        var pWriter = Packet.Of(SendOp.GameEvent);
        pWriter.Write<Command>(Command.Unknown3);
        pWriter.WriteInt(gameEvents.Count);
        foreach (GameEvent gameEvent in gameEvents) {
            pWriter.WriteUnicodeString(gameEvent.Name);
            pWriter.WriteClass<GameEventInfo>(gameEvent.EventInfo);
        }

        return pWriter;
    }
}
