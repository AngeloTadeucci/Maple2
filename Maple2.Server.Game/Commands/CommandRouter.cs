using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text;
using Autofac;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class CommandRouter {
    private readonly GameSession session;
    private ImmutableList<GameCommand> commands;
    private ImmutableDictionary<string, GameCommand> aliasLookup;
    private readonly IConsole console;
    private readonly IComponentContext context;

    public CommandRouter(GameSession session, IComponentContext context) {
        var listBuilder = ImmutableList.CreateBuilder<GameCommand>();
        var dictionaryBuilder = ImmutableDictionary.CreateBuilder<string, GameCommand>();
        foreach (GameCommand command in context.Resolve<IEnumerable<GameCommand>>(new NamedParameter("session", session))) {
            listBuilder.Add(command);
            foreach (string alias in command.Aliases) {
                dictionaryBuilder.Add(alias.ToLower(), command);
            }
        }

        this.session = session;
        this.context = context;
        commands = listBuilder.ToImmutable();
        aliasLookup = dictionaryBuilder.ToImmutable();
        console = new GameConsole(session);
    }

    public void RegisterCommands() {
        var listBuilder = ImmutableList.CreateBuilder<GameCommand>();
        var dictionaryBuilder = ImmutableDictionary.CreateBuilder<string, GameCommand>();
        foreach (GameCommand command in context.Resolve<IEnumerable<GameCommand>>(new NamedParameter("session", session))) {
            // Check permissions based on command type
            if (session.Player?.AdminPermissions.HasFlag(command.RequiredPermission) ?? false) {
                listBuilder.Add(command);
                foreach (string alias in command.Aliases) {
                    dictionaryBuilder.Add(alias.ToLower(), command);
                }
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
        if (aliasLookup.TryGetValue(commandName, out GameCommand? command)) {
            args[0] = commandName; // Ensure the command name is in the correct case so it's filtered by Invoke and doesn't show in the argument list.
            return command.Invoke(args, console);
        }

        if (session.Player.AdminPermissions == AdminPermissions.None || (commandName != "commands" && commandName != "command")) {
            session.Send(NoticePacket.Notice(NoticePacket.Flags.Message, new InterfaceText(StringCode.s_chat_unknown_command)));
            return 0;
        }

        string commandList = GetCommandList();
        if (!string.IsNullOrEmpty(commandList)) {
            console.Out.Write(commandList);
        }

        return 0;
    }

    private string GetCommandList() {
        var builder = new StringBuilder();
        builder.Append("Commands:\n");

        if (commands.Count == 0) {
            return builder.ToString();
        }

        int width = commands.Max(c => c.Name.Length);

        foreach (GameCommand command in commands) {
            if (command.IsHidden) continue;

            builder.Append($"  {command.Name.PadRight(width)}  {command.Description}\n");
        }

        builder.AppendLine();
        return builder.ToString();
    }
}

public abstract class GameCommand : Command {
    public readonly AdminPermissions RequiredPermission;
    public GameCommand(AdminPermissions permissions, string name, string? description = null) : base(name, description) {
        RequiredPermission = permissions;
    }
}
