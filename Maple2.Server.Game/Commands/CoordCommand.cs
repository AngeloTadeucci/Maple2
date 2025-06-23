using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class CoordCommand : GameCommand {
    private readonly GameSession session;

    public CoordCommand(GameSession session) : base(AdminPermissions.Debug, "coord", "Move to specified coordinates. You can use ~2, ~-2, or ~+2 to move relative to the current position.") {
        this.session = session;

        var xPosition = new Argument<string?>("x", () => null, "X Coordinate (use ~ for relative).");
        var yPosition = new Argument<string?>("y", () => null, "Y Coordinate (use ~ for relative).");
        var zPosition = new Argument<string?>("z", () => null, "Z Coordinate (use ~ for relative).");

        var force = new Option<bool>(["--force", "-f"], "Skip validation and move to the specified position.");
        var blocks = new Option<bool>(["--block", "-b"], () => false, "Interpret relative coordinates as block offsets (multiplied by block size).");

        AddArgument(xPosition);
        AddArgument(yPosition);
        AddArgument(zPosition);
        AddOption(force);
        AddOption(blocks);

        this.SetHandler<InvocationContext, string?, string?, string?, bool, bool>(Handle, xPosition, yPosition, zPosition, force, blocks);
    }

    private void Handle(InvocationContext ctx, string? x, string? y, string? z, bool force, bool blocks) {
        if (session.Field is null) return;

        Vector3 pos = session.Player.Position;
        if (x == null && y == null && z == null) {
            ctx.Console.Out.WriteLine("Current position: " + pos);
            return;
        }

        float newX = ParseCoord(x, pos.X, blocks);
        float newY = ParseCoord(y, pos.Y, blocks);
        float newZ = ParseCoord(z, pos.Z, blocks);
        var newPos = new Vector3(newX, newY, newZ);

        if (!session.Field.ValidPosition(newPos) && !force) {
            ctx.Console.Out.WriteLine("Position is invalid.");
            return;
        }

        ctx.Console.Out.WriteLine($"Moving to '{newPos}'");
        session.Send(PortalPacket.MoveByPortal(session.Player, newPos, session.Player.Rotation));
        ctx.ExitCode = 0;
        return;

        float ParseCoord(string? input, float current, bool asBlock) {
            if (string.IsNullOrEmpty(input)) return current;
            input = input.Trim();
            if (input.StartsWith('~')) {
                if (input.Length == 1) return current;
                if (float.TryParse(input[1..], out float rel)) {
                    return current + (asBlock ? rel * Constant.BlockSize : rel);
                }
                ctx.Console.Error.WriteLine($"Invalid relative coordinate: {input}. Use ~[value] or ~+[value] or ~-[value].");
                return current;
            }
            if (float.TryParse(input, out float abs)) return abs;
            ctx.Console.Error.WriteLine($"Invalid coordinate: {input}. Use a number or ~[value] for relative coordinates.");
            return current;
        }
    }
}
