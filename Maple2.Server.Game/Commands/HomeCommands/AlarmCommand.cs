using System.CommandLine;
using System.CommandLine.Invocation;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands.HomeCommands;

public class AlarmCommand : Command {
    private readonly GameSession session;

    public AlarmCommand(GameSession session) : base("hostalarm", "Send a message to all players in the map") {
        this.session = session;
        IsHidden = Constant.HideHomeCommands;

        var message = new Argument<string[]>("message", "Message to send to all players in the map");

        AddArgument(message);
        this.SetHandler<InvocationContext, string[]>(Handle, message);
    }

    private void Handle(InvocationContext context, string[] message) {
        if (session.Field is null) return;

        Character character = session.Player.Value.Character;
        if (character.MapId is not Constant.DefaultHomeMapId) {
            return;
        }

        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null || plot.OwnerId != character.AccountId) {
            return;
        }

        if (message.Length == 0) {
            session.Field.Broadcast(HomeActionPacket.HostAlarm(string.Empty));
            return;
        }

        session.Field.Broadcast(HomeActionPacket.HostAlarm(string.Join(" ", message)));
    }
}
