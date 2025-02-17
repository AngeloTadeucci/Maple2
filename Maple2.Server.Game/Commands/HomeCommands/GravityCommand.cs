using System.CommandLine;
using System.CommandLine.Invocation;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands.HomeCommands;

public class GravityCommand : Command {
    private readonly GameSession session;

    public GravityCommand(GameSession session) : base("hostgravity", "Change the gravity of the map") {
        this.session = session;
        IsHidden = Constant.HideHomeCommands;

        var gravity = new Argument<float>("gravity", "Gravity value to set");

        AddArgument(gravity);

        this.SetHandler<InvocationContext, float>(Handle, gravity);
    }

    private void Handle(InvocationContext context, float gravity) {
        Character character = session.Player.Value.Character;
        if (character.MapId is not Constant.DefaultHomeMapId) {
            return;
        }

        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null || plot.OwnerId != character.AccountId) {
            return;
        }

        gravity = Math.Min(gravity * 40, 400);
        if (gravity < 0) {
            gravity = 0;
        }

        session.Field.Broadcast(FieldPropertyPacket.Add(new FieldPropertyGravity(gravity * -1)));
        session.Field.Broadcast(NoticePacket.Notice(NoticePacket.Flags.Message, new InterfaceText(StringCode.s_ugcmap_fun_host_gravity_change)));
    }
}
