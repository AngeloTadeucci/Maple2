using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class TakeBoatHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.TakeBoat;

    public override void Handle(GameSession session, IByteReader packet) {
        int npcObjectId = packet.ReadInt();

        // do nothing? this is all handled in the job script
    }
}
