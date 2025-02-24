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
    private ImmutableList<Command> commands;
    private ImmutableDictionary<string, Command> aliasLookup;
    private readonly IConsole console;
    private readonly IComponentContext context;

    public CommandRouter(GameSession session, IComponentContext context) {
        var listBuilder = ImmutableList.CreateBuilder<Command>();
        var dictionaryBuilder = ImmutableDictionary.CreateBuilder<string, Command>();
        foreach (Command command in context.Resolve<IEnumerable<Command>>(new NamedParameter("session", session))) {
            listBuilder.Add(command);
            foreach (string alias in command.Aliases) {
                dictionaryBuilder.Add(alias.ToLower(), command);
            }
        }

        this.session = session;
        this.context = context;
        this.commands = listBuilder.ToImmutable();
        this.aliasLookup = dictionaryBuilder.ToImmutable();
        this.console = new GameConsole(session);
    }

    public void RegisterCommands() {
        var listBuilder = ImmutableList.CreateBuilder<Command>();
        var dictionaryBuilder = ImmutableDictionary.CreateBuilder<string, Command>();
        foreach (Command command in context.Resolve<IEnumerable<Command>>(new NamedParameter("session", session))) {
            listBuilder.Add(command);
            foreach (string alias in command.Aliases) {
                dictionaryBuilder.Add(alias, command);
            }
        }

        commands = listBuilder.ToImmutable();
        aliasLookup = dictionaryBuilder.ToImmutable();
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

        string commandName = args[0].ToLower();
        if (aliasLookup.TryGetValue(commandName, out Command? command)) {
            args[0] = commandName; // Ensure the command name is in the correct case so it's filtered by Invoke and doesn't show in the argument list.
            return command.Invoke(args, console);
        }

        if (commandName != "help") {
            console.Error.WriteLine($"Unrecognized command '{commandName}'");
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
