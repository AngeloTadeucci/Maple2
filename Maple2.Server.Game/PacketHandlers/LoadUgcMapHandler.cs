using System.Diagnostics;
using System.Numerics;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;
using Serilog;

namespace Maple2.Server.Game.PacketHandlers;

public class LoadUgcMapHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.RequestLoadUgcMap;

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required GameStorage GameStorage { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        Debug.Assert(packet.ReadInt() == GameSession.FIELD_KEY);
        if (session.Field == null) {
            return;
        }

        List<PlotCube> plotCubes = [];
        foreach (Plot plot in session.Field.Plots.Values) {
            foreach (PlotCube cube in plot.Cubes.Values) {
                cube.PlotId = plot.Number;
                plotCubes.Add(cube);
            }
        }

        if (session.Field.MapId != Constant.DefaultHomeMapId || session.Field.OwnerId <= 0) {
            session.Send(LoadUgcMapPacket.Load(plotCubes.Count));

            LoadPlots(session, plotCubes);
            return;
        }

        using GameStorage.Request db = GameStorage.Context();
        Home? home = db.GetHome(session.Field.OwnerId);
        if (home == null) {
            return;
        }

        // TODO: Check if plot has entry points

        // plots start at 0,0 and are built towards negative x and y
        int dimension = -1 * (home.Area - 1);

        // find the blocks in most negative x,y direction, with the highest z value
        int height = 0;
        if (plotCubes.Count > 0) {
            height = plotCubes.Where(cube => cube.Position.X == dimension && cube.Position.Y == dimension)
                .Max(cube => cube.Position.Z);
        }

        dimension *= VectorExtensions.BLOCK_SIZE;

        height++; // add 1 to height to be on top of the block
        height *= VectorExtensions.BLOCK_SIZE;
        session.Player.Position = new Vector3(dimension, dimension, height + 1);

        // Technically this sends home details to all players who enter map (including passcode)
        // but you would already know passcode if you entered the map.
        session.Send(LoadUgcMapPacket.LoadHome(plotCubes.Count, home));

        LoadPlots(session, plotCubes);

        // this is a workaround for the client to load the map before field add player - without this, player will fall and get tp'd back to 0,0
        Task.Delay(200).Wait();
    }

    private static void LoadPlots(GameSession session, List<PlotCube> plotCubes) {
        lock (session.Field.Plots) {
            session.Send(LoadCubesPacket.PlotOwners(session.Field.Plots.Values));
            session.Send(LoadCubesPacket.Load(plotCubes));
            session.Send(LoadCubesPacket.PlotState([.. session.Field.Plots.Values]));
            session.Send(LoadCubesPacket.PlotExpiry([.. session.Field.Plots.Values]));
        }
    }
}
