using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class TombstoneHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.Tombstone;

    public override void Handle(GameSession session, IByteReader packet) {
        int objectId = packet.ReadInt();
        int hits = packet.ReadInt();

        if (!session.Field.TryGetPlayer(objectId, out FieldPlayer? player) || player.Tombstone == null) {
            return;
        }

        player.Tombstone.HitsRemaining -= (byte) hits;
        session.ConditionUpdate(ConditionType.hit_tombstone);
    }
}
