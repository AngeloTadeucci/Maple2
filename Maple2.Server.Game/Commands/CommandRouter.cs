using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Text;
using Autofac;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Packets;
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
            // Check permissions based on command type
            bool hasPermission = command switch {
                AnimateNpcCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.Debug) ?? false,
                AlertCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.Alert) ?? false,
                BuffCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.GameMaster) ?? false,
                CoordCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.Debug) ?? false,
                DailyResetCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.EventManagement) ?? false,
                DebugCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.Debug) ?? false,
                FieldCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.Debug) ?? false,
                FindCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.Find) ?? false,
                FreeCamCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.Debug) ?? false,
                HomeCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.Debug) ?? false,
                GotoCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.Warp) ?? false,
                ItemCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.SpawnItem) ?? false,
                KillCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.GameMaster) ?? false,
                NpcCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.SpawnNpc) ?? false,
                PetCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.SpawnNpc) ?? false,
                PlayerCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.PlayerCommands) ?? false,
                QuestCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.Quest) ?? false,
                StringBoardCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.StringBoard) ?? false,
                TriggerCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.Debug) ?? false,
                TutorialCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.Debug) ?? false,
                WarpCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.Warp) ?? false,
                AdminPermissionCommand => session.Player?.AdminPermissions.HasFlag(AdminPermissions.Admin) ?? false,
                // Add other command types as needed
                _ => true // Default to allowing command if not specifically restricted
            };

            if (hasPermission) {
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
        if (aliasLookup.TryGetValue(commandName, out Command? command)) {
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
        int width = commands.Max(c => c.Name.Length);

        if (commands.Count == 0) {
            return string.Empty;
        }

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
