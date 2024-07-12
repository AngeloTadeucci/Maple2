using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;
public class FileHashHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.FileHash;

    public override void Handle(GameSession session, IByteReader packet) {
        System.Console.WriteLine(packet);
        packet.ReadInt();
        string filename = packet.ReadString();
        string md5 = packet.ReadString();

        System.Console.WriteLine($"Hash for {filename}: {md5}");
    }
}