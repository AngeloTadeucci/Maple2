using System.CommandLine;
using System.CommandLine.Invocation;
using System.Numerics;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands.HomeCommands;

public class BallCommand : Command {
    private readonly GameSession session;

    public BallCommand(GameSession session) : base("hostball", "Spawn a ball in the map") {
        this.session = session;
        IsHidden = Constant.HideHomeCommands;

        var ballSize = new Argument<float>("size", () => 1, "Size of the ball");

        AddArgument(ballSize);
        this.SetHandler<InvocationContext, float>(Handle, ballSize);
    }

    private void Handle(InvocationContext context, float size) {
        if (session.Field is null) return;

        Character character = session.Player.Value.Character;
        if (character.MapId is not Constant.DefaultHomeMapId) {
            return;
        }

        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null || plot.OwnerId != character.AccountId) {
            return;
        }

        FieldGuideObject? guideObject = session.GuideObject;
        if (guideObject is not null) {
            if (guideObject.Value is not BallGuideObject) return;

            session.GuideObject = null;
            session.Field.Broadcast(HomeActionPacket.RemoveBall(guideObject));
            return;
        }

        size = Math.Min(30 + size * 30, 330);
        if (size < 0) {
            size = 60;
        }

        session.GuideObject = session.Field.SpawnGuideObject(session.Player, new BallGuideObject(size));
        Vector3 playerPosition = session.Player.Position;
        session.GuideObject.Position = playerPosition with {
            Z = playerPosition.Z + 2 * Constant.BlockSize,
        };

        session.Field.Broadcast(HomeActionPacket.AddBall(session.GuideObject));
    }
}
