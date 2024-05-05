using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Text;
using Autofac;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class CommandRouter {
    private readonly GameSession session;
    private readonly ImmutableList<Command> commands;
    private readonly ImmutableDictionary<string, Command> aliasLookup;
    private readonly IConsole console;

    public CommandRouter(GameSession session, IComponentContext context) {
        var listBuilder = ImmutableList.CreateBuilder<Command>();
        var dictionaryBuilder = ImmutableDictionary.CreateBuilder<string, Command>();
        foreach (Command command in context.Resolve<IEnumerable<Command>>(new NamedParameter("session", session))) {
            listBuilder.Add(command);
            foreach (string alias in command.Aliases) {
                dictionaryBuilder.Add(alias, command);
            }
        }

        this.session = session;
        this.commands = listBuilder.ToImmutable();
        this.aliasLookup = dictionaryBuilder.ToImmutable();
        this.console = new GameConsole(session);
    }

    public int Invoke(string commandLine) {
        return Invoke(CommandLineStringSplitter.Instance.Split(commandLine).ToArray());
    }

    private int Invoke(string[] args) {
        if (args.Length == 0) {
            return 0;
        }

        // Ignore commands before loaded in game.
        if (session.Field == null) {
            return 0;
        }

        if (aliasLookup.TryGetValue(args[0], out Command? command)) {
            return command.Invoke(args, console);
        }

        if (args[0] != "help") {
            console.Error.WriteLine($"Unrecognized command '{args[0]}'");
        }

        console.Out.Write(GetCommandList());
        return 0;
    }

    private string GetCommandList() {
        int width = commands.Max(c => c.Name.Length);

        var builder = new StringBuilder();
        builder.Append("Commands:\n");
        foreach (Command command in commands) {
            if (command.IsHidden) continue;

            builder.Append($"  {command.Name.PadRight(width)}  {command.Description}\n");
        }

        builder.AppendLine();
        return builder.ToString();
    }
}
