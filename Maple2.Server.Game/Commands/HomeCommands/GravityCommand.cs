using System.CommandLine;
using System.CommandLine.Invocation;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands.HomeCommands;

public class GravityCommand : GameCommand {
    private readonly GameSession session;

    private const float GravityMultiplier = 40f;
    private const float MaxGravity = 400f;

    public GravityCommand(GameSession session) : base(AdminPermissions.None, "hostgravity", "Change the gravity of the map") {
        this.session = session;
        IsHidden = Constant.HideHomeCommands;

        var gravity = new Argument<float>("gravity", "Gravity value to set");

        AddArgument(gravity);

        this.SetHandler<InvocationContext, float>(Handle, gravity);
    }

    private void Handle(InvocationContext context, float gravity) {
        if (session.Field is null) return;
        Character character = session.Player.Value.Character;
        if (character.MapId is not Constant.DefaultHomeMapId) {
            return;
        }

        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null || plot.OwnerId != character.AccountId) {
            return;
        }

        // Convert user-friendly gravity (0-10) to game units (0-400)
        gravity = Math.Min(gravity * GravityMultiplier, MaxGravity);
        if (gravity < 0) {
            gravity = 0;
        }

        session.Field.Broadcast(FieldPropertyPacket.Add(new FieldPropertyGravity(gravity * -1)));
        session.Field.Broadcast(NoticePacket.Notice(NoticePacket.Flags.Message, new InterfaceText(StringCode.s_ugcmap_fun_host_gravity_change)));
    }
}
