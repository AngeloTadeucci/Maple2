using Maple2.Model.Game.Event;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Core.Packets;

public static class GameEventPacket {
    private enum Command : byte {
        Load = 0,
        Add = 1,
        Remove = 2,
        Reload = 3,
    }

    public static ByteWriter Load(IList<GameEvent> gameEvents) {
        var pWriter = Packet.Of(SendOp.GameEvent);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(gameEvents.Count);
        foreach (GameEvent gameEvent in gameEvents) {
            pWriter.WriteUnicodeString(gameEvent.Name);
            pWriter.WriteClass<GameEvent>(gameEvent);
        }

        return pWriter;
    }

    public static ByteWriter Test() {
        var pWriter = Packet.Of(SendOp.GameEvent);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(1);
        pWriter.WriteUnicodeString("ReturnUser");
        pWriter.WriteInt(40);
        pWriter.WriteInt(94000265);
        pWriter.WriteLong(1696040439);
        pWriter.WriteLong(1759198839);
        pWriter.WriteInt(1);
        pWriter.WriteInt(74000001);

        return pWriter;
    }

    public static ByteWriter Add(IList<GameEvent> gameEvents) {
        var pWriter = Packet.Of(SendOp.GameEvent);
        pWriter.Write<Command>(Command.Add);
        pWriter.WriteInt(gameEvents.Count);
        foreach (GameEvent gameEvent in gameEvents) {
            pWriter.WriteUnicodeString(gameEvent.Name);
            pWriter.WriteClass<GameEvent>(gameEvent);
        }

        return pWriter;
    }

    public static ByteWriter Remove(IList<int> eventIds) {
        var pWriter = Packet.Of(SendOp.GameEvent);
        pWriter.Write<Command>(Command.Remove);
        pWriter.WriteInt(eventIds.Count);
        foreach (int value in eventIds) {
            pWriter.WriteInt(value);
        }

        return pWriter;
    }

    public static ByteWriter Reload(IList<GameEvent> gameEvents) {
        var pWriter = Packet.Of(SendOp.GameEvent);
        pWriter.Write<Command>(Command.Reload);
        pWriter.WriteInt(gameEvents.Count);
        foreach (GameEvent gameEvent in gameEvents) {
            pWriter.WriteUnicodeString(gameEvent.Name);
            pWriter.WriteClass<GameEvent>(gameEvent);
        }

        return pWriter;
    }
}
