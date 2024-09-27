using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class HomeActionHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.HomeAction;

    private enum Command : byte {
        Smite = 1,
        Kick = 2,
        Survey = 5,
        ChangePortalSettings = 6,
        UpdateBallCoord = 7,
        SendPortalSettings = 13,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.ChangePortalSettings:
                HandleChangePortalSettings(session, packet);
                break;
            case Command.SendPortalSettings:
                HandleSendPortalSettings(session, packet);
                break;
        }
    }
    private void HandleChangePortalSettings(GameSession session, IByteReader packet) {
        packet.ReadByte();
        Vector3B coord = packet.Read<Vector3B>();

        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null) {
            return;
        }

        if (!plot.Cubes.TryGetValue(coord, out PlotCube? cube)) {
            Logger.Warning("Cube not found at {0}", coord);
            return;
        }

        if (cube.ItemId is not Constant.InteriorPortalCubeId) {
            Logger.Warning("Cube is not a portal cube at {0}", coord);
            return;
        }

        if (cube.PortalSettings is null) {
            Logger.Warning("Cube does not have portal settings at {0}", coord);
            return;
        }

        cube.PortalSettings.PortalName = packet.ReadUnicodeString();
        cube.PortalSettings.Method = (PortalActionType) packet.ReadByte();
        cube.PortalSettings.Destination = (CubePortalDestination) packet.ReadByte();
        cube.PortalSettings.DestinationTarget = packet.ReadUnicodeString();

        foreach (FieldPortal portal in session.Field.GetPortals()) {
            session.Field.RemovePortal(portal.ObjectId);
        }

        List<PlotCube> cubePortals = plot.Cubes.Values
            .Where(x => x.ItemId is Constant.InteriorPortalCubeId && x.PortalSettings is not null)
            .ToList();

        foreach (PlotCube cubePortal in cubePortals) {
            FieldPortal fieldPortal = session.Field.SpawnCubePortal(cubePortal);
            session.Field.Broadcast(PortalPacket.Add(fieldPortal));
        }

        session.Housing.SaveHome();
    }

    private void HandleSendPortalSettings(GameSession session, IByteReader packet) {
        var coord = packet.Read<Vector3B>();

        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null) {
            return;
        }

        if (!plot.Cubes.TryGetValue(coord, out PlotCube? cube)) {
            Logger.Warning("Cube not found at {0}", coord);
            return;
        }

        if (cube.ItemId is not Constant.InteriorPortalCubeId) {
            Logger.Warning("Cube is not a portal cube at {0}", coord);
            return;
        }

        List<string> otherPortalsNames = plot.Cubes.Values
            .Where(x => x.ItemId is Constant.InteriorPortalCubeId && x.Id != cube.Id)
            .Select(x => x.PortalSettings?.PortalName ?? string.Empty)
            .ToList();

        session.Send(HomeActionPacket.SendCubePortalSettings(cube, otherPortalsNames));
    }
}
