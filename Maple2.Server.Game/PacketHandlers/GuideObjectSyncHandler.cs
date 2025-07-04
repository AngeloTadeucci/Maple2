using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class GuideObjectSyncHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.GuideObjectSync;

    public override void Handle(GameSession session, IByteReader packet) {
        if (session.State != SessionState.Connected || session.Field == null || session.GuideObject == null) {
            return;
        }

        var type = packet.Read<GuideObjectType>();
        if (type != session.GuideObject.Value.Type) {
            return;
        }

        byte segments = packet.ReadByte();
        if (segments == 0) {
            return;
        }

        var stateSyncs = new StateSync[segments];
        for (int i = 0; i < segments; i++) {
            var playerState = packet.Peek<ActorState>();
            StateSync stateSync = playerState switch {
                ActorState.MicroGameRps => new StateSyncRps(),
                ActorState.MicroGameCoupleDance => new StateSyncCoupleDance(),
                ActorState.WeddingEmotion => new StateSyncWeddingEmotion(),
                _ => new StateSync(),
            };

            stateSync.ReadFrom(packet);
            stateSyncs[i] = stateSync;

            packet.ReadInt(); // ClientTicks
            packet.ReadInt(); // ServerTicks
        }

        using (var buffer = new PoolByteWriter()) {
            buffer.GuideObject(session.GuideObject.ObjectId, stateSyncs);
            session.Field.Broadcast(buffer, sender: session);
        }

        session.GuideObject.Position = stateSyncs[^1].Position;
        session.GuideObject.Rotation = new Vector3(0, 0, stateSyncs[^1].Rotation);
    }
}
