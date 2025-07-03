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

    public static ByteWriter Load(params GameEvent[] gameEvents) {
        var pWriter = Packet.Of(SendOp.GameEvent);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(gameEvents.Length);
        foreach (GameEvent gameEvent in gameEvents) {
            pWriter.WriteClass<GameEvent>(gameEvent);
        }

        return pWriter;
    }

    public static ByteWriter Add(params GameEvent[] gameEvents) {
        var pWriter = Packet.Of(SendOp.GameEvent);
        pWriter.Write<Command>(Command.Add);
        pWriter.WriteInt(gameEvents.Length);
        foreach (GameEvent gameEvent in gameEvents) {
            pWriter.WriteClass<GameEvent>(gameEvent);
        }

        return pWriter;
    }

    public static ByteWriter Remove(params int[] eventIds) {
        var pWriter = Packet.Of(SendOp.GameEvent);
        pWriter.Write<Command>(Command.Remove);
        pWriter.WriteInt(eventIds.Length);
        foreach (int value in eventIds) {
            pWriter.WriteInt(value);
        }

        return pWriter;
    }

    public static ByteWriter Reload(params GameEvent[] gameEvents) {
        var pWriter = Packet.Of(SendOp.GameEvent);
        pWriter.Write<Command>(Command.Reload);
        pWriter.WriteInt(gameEvents.Length);
        foreach (GameEvent gameEvent in gameEvents) {
            pWriter.WriteClass<GameEvent>(gameEvent);
        }

        return pWriter;
    }
}
