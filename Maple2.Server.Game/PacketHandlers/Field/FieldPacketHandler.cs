using System.Buffers;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers.Field;

// Safe for use for freely modifying state on Field. Handlers are run on Field's thread
public abstract class FieldPacketHandler : PacketHandler<GameSession> {
    protected FieldPacketHandler() { }

    public override bool TryHandleDeferred(GameSession session, IByteReader reader) {
        if (reader is not ByteReader packet) {
            return false;
        }

        if (session.Field is null) {
            return false;
        }

        byte[] bufferCopy = ArrayPool<byte>.Shared.Rent(packet.Length);
        Array.Copy(packet.Buffer, bufferCopy, packet.Length);
        var packetCopy = new ByteReader(bufferCopy, packet.Position);

        session.Field.QueuePacket(this, session, packetCopy);

        return true;
    }
}
