using System.CommandLine;
using System.CommandLine.Invocation;
using Maple2.Model.Enum;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class TitleCommand : GameCommand {
    private readonly GameSession session;
    public TitleCommand(GameSession session) : base(AdminPermissions.GameMaster, "title", "Gain the title") {
        this.session = session;

        var titleId = new Argument<int>("titleId", "The ID of the title to gain");
        AddArgument(titleId);
        this.SetHandler<InvocationContext, int>(Handle, titleId);
    }

    private void Handle(InvocationContext context, int titleId) {
        if (session.Field == null) return;

        if (!session.Player.Value.Unlock.Titles.Add(titleId)) {
            context.Console.WriteLine($"You already have the title {titleId}.");
            return;
        }

        session.Send(UserEnvPacket.AddTitle(titleId));
    }
}
