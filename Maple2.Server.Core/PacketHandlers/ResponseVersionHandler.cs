using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Network;

namespace Maple2.Server.Core.PacketHandlers;

public abstract class ResponseVersionHandler<T> : PacketHandler<T> where T : Session {
    public override RecvOp OpCode => RecvOp.ResponseVersion;

    public override void Handle(T session, IByteReader packet) {
        uint version = packet.Read<uint>();
        packet.ReadShort(); // 47
        var locale = packet.Read<Locale>();

        if (version != Session.VERSION || locale != Locale.NA) {
            session.Disconnect();
        }
    }
}
