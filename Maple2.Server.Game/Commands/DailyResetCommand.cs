using System.CommandLine;
using System.CommandLine.Invocation;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class DailyResetCommand : Command {
    private readonly GameSession session;
    private const string NAME = "daily-reset";
    private const string DESCRIPTION = "Force daily reset for this player.";

    public DailyResetCommand(GameSession session) : base(NAME, DESCRIPTION) {
        this.session = session;
        this.SetHandler<InvocationContext>(Handle);
    }

    private void Handle(InvocationContext ctx) {
        session.DailyReset();
    }
}
