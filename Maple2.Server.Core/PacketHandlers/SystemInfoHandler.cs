using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Network;

namespace Maple2.Server.Core.PacketHandlers;

public abstract class SystemInfoHandler<T> : PacketHandler<T> where T : Session {
    public override RecvOp OpCode => RecvOp.SystemInfo;

    public override void Handle(T session, IByteReader packet) {
        string info = packet.ReadUnicodeString();
        Logger.Debug("System Info: {Info}", info);
    }
}
