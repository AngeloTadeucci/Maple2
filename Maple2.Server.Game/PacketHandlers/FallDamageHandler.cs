using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class FallDamageHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.StateFallDamage;

    private const float BASE_FALL_DISTANCE = Constant.BlockSize * 5;

    public override void Handle(GameSession session, IByteReader packet) {
        float distance = packet.ReadFloat();
        distance -= 1000f;
        if (distance <= 0) {
            return;
        }

        session.Player.FallDamage(distance);
    }
}
