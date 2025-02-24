using System.Numerics;
using Maple2.Database.Storage;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Model.Metadata.FieldEntity;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Collision;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.PacketHandlers;

public class FishingHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.Fishing;

    private enum Command : byte {
        Prepare = 0,
        Stop = 1,
        Catch = 8,
        Start = 9,
        FailMinigame = 10,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Prepare:
                HandlePrepare(session, packet);
                break;
            case Command.Stop:
                HandleStop(session);
                break;
            case Command.Catch:
                HandleCatch(session, packet);
                break;
            case Command.Start:
                HandleStart(session, packet);
                break;
            case Command.FailMinigame:
                HandleFailMinigame(session);
                break;
        }
    }

    private void HandlePrepare(GameSession session, IByteReader packet) {
        long fishingRodUid = packet.ReadLong();

        FishingError error = session.Fishing.Prepare(fishingRodUid);
        if (error != FishingError.none) {
            session.Send(FishingPacket.Error(error));
        }
    }



    private static void HandleStop(GameSession session) {
        if (session.Field == null || session.GuideObject == null) {
            return;
        }


        session.Fishing.Reset();
    }

    private void HandleCatch(GameSession session, IByteReader packet) {
        if (session.Field == null || session.GuideObject?.Value is not FishingGuideObject fishingGuideObject) {
            return;
        }

        bool success = packet.ReadBool();

        FishingError error = session.Fishing.Catch(success);
        if (error != FishingError.none) {
            session.Send(FishingPacket.Error(error));
        }
    }

    private void HandleStart(GameSession session, IByteReader packet) {
        if (session.Field == null || session.GuideObject?.Value is not FishingGuideObject fishingGuideObject) {
            return;
        }

        var fishingBlock = packet.Read<Vector3B>();

        FishingError error = session.Fishing.Start(fishingBlock);
        if (error != FishingError.none) {
            session.Send(FishingPacket.Error(error));
        }
    }

    private static void HandleFailMinigame(GameSession session) {
        Console.WriteLine("Fail Minigame");
        session.Fishing.FailMinigame();
    }
}
