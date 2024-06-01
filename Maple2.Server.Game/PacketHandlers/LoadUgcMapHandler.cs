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

        if (session.Field.MapId != Constant.DefaultHomeMapId || session.Field.OwnerId <= 0) {
            session.Send(LoadUgcMapPacket.Load());
            return;
        }

        using GameStorage.Request db = GameStorage.Context();
        Home? home = db.GetHome(session.Field.OwnerId);
        if (home == null) {
            return;
        }

        session.Field.Plots.TryGetValue(home.Indoor.Number, out Plot? plot);
        if (plot == null) {
            Log.Error("Plot not found for home {HomeId}", home.Indoor.Number);
            return;
        }

        // TODO: Check if plot has entry points

        // plots start at 0,0 and are built towards negative x and y
        int x = -1 * (home.Area - 1);

        // find the blocks in most negative x,y direction, with the highest z value
        int height = plot.Cubes.Where(cube => cube.Key.X == x && cube.Key.Y == x).Max(cube => cube.Key.Z);

        x *= VectorExtensions.BLOCK_SIZE;

        height++; // add 1 to height to be on top of the block
        height *= VectorExtensions.BLOCK_SIZE;
        session.Player.Position = new Vector3(x, x, height + 1);

        // Technically this sends home details to all players who enter map (including passcode)
        // but you would already know passcode if you entered the map.
        session.Send(LoadUgcMapPacket.LoadHome(home));

        // TODO: Rework this to support outdoor plots
        session.Send(LoadCubesPacket.PlotOwners(session.Field.Plots.Values));
        lock (session.Field.Plots) {
            foreach (Plot fieldPlot in session.Field.Plots.Values) {
                if (fieldPlot.Cubes.Count > 0) {
                    session.Send(LoadCubesPacket.Load(fieldPlot));
                }
            }

            Plot[] ownedPlots = session.Field.Plots.Values.Where(plot => plot.State != PlotState.Open).ToArray();
            if (ownedPlots.Length > 0) {
                session.Send(LoadCubesPacket.PlotState(ownedPlots));
                session.Send(LoadCubesPacket.PlotExpiry(ownedPlots));
            }
        }

        // this is a workaround for the client to load the map before field add player - without this, player will fall and get tp'd back to 0,0
        Task.Delay(200).Wait();
    }
}
