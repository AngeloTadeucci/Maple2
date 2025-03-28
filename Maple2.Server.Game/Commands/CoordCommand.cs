using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Numerics;
using DotRecast.Core.Numerics;
using Maple2.Database.Storage;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.DotRecast;

namespace Maple2.Server.Game.Commands;

public class CoordCommand : Command {
    private const string NAME = "coord";
    private const string DESCRIPTION = "Move to specified coordinates. If no coordinates are provided, the current position is displayed.";

    private readonly GameSession session;
    private readonly MapMetadataStorage mapStorage;

    public CoordCommand(GameSession session, MapMetadataStorage mapStorage) : base(NAME, DESCRIPTION) {
        this.session = session;
        this.mapStorage = mapStorage;

        var xPosition = new Argument<int?>("x", () => null, "X Coordinate.");
        var yPosition = new Argument<int?>("y", () => null, "Y Coordinate.");
        var zPosition = new Argument<int?>("z", () => null, "Z Coordinate.");

        var force = new Option<bool>(["--force", "-f"], "Skip validation and move to the specified position.");

        AddArgument(xPosition);
        AddArgument(yPosition);
        AddArgument(zPosition);
        AddOption(force);
        this.SetHandler<InvocationContext, int?, int?, int?, bool>(Handle, xPosition, yPosition, zPosition, force);
    }

    private void Handle(InvocationContext ctx, int? x, int? y, int? z, bool force) {
        if (x is null || y is null || z is null) {
            ctx.Console.Out.WriteLine("Current position: " + session.Player.Position);
            return;
        }
        Vector3 position = new((int) x, (int) y, (int) z);

        if (!session.Field.ValidPosition(position) && !force) {
            ctx.Console.Out.WriteLine("Position is invalid.");
            return;
        }

        try {
            ctx.Console.Out.WriteLine($"Moving to '{position}'");
            session.Send(PortalPacket.MoveByPortal(session.Player, position, session.Player.Rotation));
            ctx.ExitCode = 0;
        } catch (SystemException ex) {
            ctx.Console.Error.WriteLine(ex.Message);
            ctx.ExitCode = 1;
        }
    }
}
