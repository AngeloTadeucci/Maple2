using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Text;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class AdminPermissionCommand : GameCommand {
    private const string NAME = "perm";
    private const string DESCRIPTION = "Set/Remove admin permissions.";
    public const AdminPermissions RequiredPermission = AdminPermissions.Admin;

    public AdminPermissionCommand(GameSession session) : base(RequiredPermission, NAME, DESCRIPTION) {
        AddCommand(new SetCommand(session));
        AddCommand(new RemoveCommand(session));
        AddCommand(new ViewCommand(session));
    }

    private class SetCommand : Command {
        private readonly GameSession session;

        public SetCommand(GameSession session) : base("set", "Set permission on a player.") {
            this.session = session;

            var player = new Argument<string>("player", "Player Name.");
            var permFlag = new Argument<int>("flag", $"Permission Flag. Possible flags:\n"
                                                     + $"{GetFlagsString()}");

            AddArgument(player);
            AddArgument(permFlag);
            this.SetHandler<InvocationContext, string, int>(Handle, player, permFlag);
        }

        private void Handle(InvocationContext ctx, string playerName, int permFlag) {
            if (permFlag == 0) {
                ctx.Console.Out.WriteLine("Flag cannot be 0.");
                return;
            }

            if (string.IsNullOrEmpty(playerName)) {
                ctx.Console.Out.WriteLine("Player name cannot be empty.");
                return;
            }

            var flag = (AdminPermissions) permFlag;
            if (!Enum.IsDefined<AdminPermissions>(flag)) {
                ctx.Console.Out.WriteLine($"Invalid flag: {permFlag}");
                return;
            }

            FieldPlayer? player = session.Field.GetPlayers().Values
                .FirstOrDefault(player => string.Equals(player.Value.Character.Name, playerName, StringComparison.OrdinalIgnoreCase));
            if (player is null) {
                ctx.Console.Out.WriteLine($"Player {playerName} not found in field.");
                return;
            }
            // Set this to get correct casing
            playerName = player.Value.Character.Name;

            player.AdminPermissions |= flag;
            player.Session.CommandHandler.RegisterCommands();
            ctx.Console.Out.WriteLine($"Permission {flag} set on {playerName}.");
            player.Session.Send(NoticePacket.Notice(NoticePacket.Flags.MessageBox | NoticePacket.Flags.Message, new InterfaceText($"<FONT size='18'>Granted admin permission: <FONT color='#73eafd'>{flag.ToString()}</FONT></FONT>", true)));
        }
    }

    private class RemoveCommand : Command {
        private readonly GameSession session;

        public RemoveCommand(GameSession session) : base("remove", "Remove permission on a player.") {
            this.session = session;

            var player = new Argument<string>("player", "Player Name.");
            var permFlag = new Argument<int>("flag", $"Permission Flag. Possible flags:\n"
                                                     + $"{GetFlagsString()}");

            AddArgument(player);
            AddArgument(permFlag);
            this.SetHandler<InvocationContext, string, int>(Handle, player, permFlag);
        }

        private void Handle(InvocationContext ctx, string playerName, int permFlag) {
            if (permFlag == 0) {
                ctx.Console.Out.WriteLine("Flag cannot be 0.");
                return;
            }

            if (string.IsNullOrEmpty(playerName)) {
                ctx.Console.Out.WriteLine("Player name cannot be empty.");
                return;
            }

            var flag = (AdminPermissions) permFlag;
            if (!Enum.IsDefined<AdminPermissions>(flag)) {
                ctx.Console.Out.WriteLine($"Invalid flag: {permFlag}");
                return;
            }

            FieldPlayer? player = session.Field.GetPlayers().Values
                .FirstOrDefault(player => string.Equals(player.Value.Character.Name, playerName, StringComparison.OrdinalIgnoreCase));
            if (player is null) {
                ctx.Console.Out.WriteLine($"Player {playerName} not found in field.");
                return;
            }
            // Set this to get correct casing
            playerName = player.Value.Character.Name;

            if (player.AdminPermissions == AdminPermissions.Admin) {
                ctx.Console.Out.WriteLine($"Cannot remove admin permissions from {playerName}.");
                return;
            }

            player.AdminPermissions &= ~flag;
            player.Session.CommandHandler.RegisterCommands();
            ctx.Console.Out.WriteLine($"Permission {flag} removed from {playerName}.");
        }
    }

    private class ViewCommand : Command {
        private readonly GameSession session;

        public ViewCommand(GameSession session) : base("view", "View permissions on a player.") {
            this.session = session;

            var player = new Argument<string>("player", "Player Name.");

            AddArgument(player);
            this.SetHandler<InvocationContext, string>(Handle, player);
        }

        private void Handle(InvocationContext ctx, string playerName) {
            if (string.IsNullOrEmpty(playerName)) {
                ctx.Console.Out.WriteLine("Player name cannot be empty.");
                return;
            }

            using GameStorage.Request db = session.GameStorage.Context();
            long characterId = db.GetCharacterId(playerName);
            if (characterId == 0) {
                ctx.Console.Out.WriteLine($"Player '{playerName}' not found.");
                return;
            }

            if (!session.PlayerInfo.GetOrFetch(characterId, out PlayerInfo? playerInfo)) {
                ctx.Console.Out.WriteLine($"Player '{playerName}' not found.");
                return;
            }

            // Set this to get correct casing
            playerName = playerInfo.Name;
            long accountId = playerInfo.AccountId;
            if (accountId == 0) {
                ctx.Console.Out.WriteLine($"Player '{playerName}' not found.");
                return;
            }

            Account? account = db.GetAccount(accountId);
            if (account is null) {
                ctx.Console.Out.WriteLine($"Player '{playerName}' not found.");
                return;
            }

            ctx.Console.Out.WriteLine($"Permissions for {playerName}: \n"
                                      + $"{account.AdminPermissions}");
        }
    }

    private static string GetFlagsString() {
        var result = new StringBuilder();
        foreach (AdminPermissions flag in Enum.GetValues(typeof(AdminPermissions))) {
            if (flag == AdminPermissions.GameMaster || flag == AdminPermissions.Admin) {
                continue; // Skip composite permissions
            }

            result.AppendLine($"{flag} ({(int) flag})");
        }

        // Add composite flags at the end
        result.AppendLine($"GameMaster ({(int) AdminPermissions.GameMaster})");
        result.AppendLine($"Admin ({(int) AdminPermissions.Admin})");

        return result.ToString().TrimEnd();
    }
}
