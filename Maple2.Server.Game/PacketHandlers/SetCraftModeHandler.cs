using System.Diagnostics;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.PacketHandlers;

public class SetCraftModeHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.RequestSetCraftMode;

    private enum Command : byte {
        Stop = 0,
        Start = 1,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Stop:
                HandleStop(session, packet);
                return;
            case Command.Start:
                HandleStart(session, packet);
                return;
        }
    }

    private void HandleStop(GameSession session, IByteReader packet) {
        bool isStart = packet.ReadBool();
        Debug.Assert(!isStart);

        if (session.Field == null || session.GuideObject == null) {
            return;
        }

        session.Field.Broadcast(SetCraftModePacket.Stop(session.Player.ObjectId));
        session.Field.Broadcast(GuideObjectPacket.Remove(session.GuideObject));
        session.GuideObject = null;
        session.HeldCube = null;
    }

    private void HandleStart(GameSession session, IByteReader packet) {
        bool isStart = packet.ReadBool();
        Debug.Assert(isStart);

        if (session.Field == null || session.GuideObject != null) {
            return;
        }

        var firstCubeItem = packet.ReadClass<PlotCube>();

        session.GuideObject = session.Field.SpawnGuideObject(session.Player, new ConstructionGuideObject());
        session.HeldCube = firstCubeItem;

        session.Field.Broadcast(GuideObjectPacket.Create(session.GuideObject));
        session.Field.Broadcast(SetCraftModePacket.Plot(session.Player.ObjectId, firstCubeItem));
    }
}
