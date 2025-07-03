using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class BuffCommand : GameCommand {
    private readonly GameSession session;
    private readonly SkillMetadataStorage skillStorage;

    public BuffCommand(GameSession session, SkillMetadataStorage skillStorage) : base(AdminPermissions.GameMaster, "buff", "Add buff to player.") {
        this.session = session;
        this.skillStorage = skillStorage;

        var id = new Argument<int>("id", "Id of buff to activate.");
        var level = new Option<int>(["--level", "-l"], () => 1, "Buff level.");
        var stack = new Option<int>(["--stack", "-s"], () => 1, "Amount of stacks on the buff.");
        var duration = new Option<int>(["--duration", "-d"], () => -1, "Duration of the buff in seconds.");
        var all = new Option<bool>(["--all", "-a"], () => false, "Apply to all players in the field.");
        var target = new Option<string>(["--target", "-t"], () => string.Empty, "Target player by name.");
        var remove = new Option<bool>(["--remove", "-r"], () => false, "Remove buff from target.");

        AddArgument(id);
        AddOption(level);
        AddOption(stack);
        AddOption(duration);
        AddOption(all);
        AddOption(target);
        AddOption(remove);
        this.SetHandler<InvocationContext, int, int, int, int, bool, string, bool>(Handle, id, level, stack, duration, all, target, remove);
    }

    private void Handle(InvocationContext ctx, int buffId, int level, int stack, int duration, bool all, string target, bool remove) {
        if (session.Field is null) return;
        try {
            if (!skillStorage.TryGetEffect(buffId, (short) level, out AdditionalEffectMetadata? _)) {
                ctx.Console.Error.WriteLine($"Invalid buff: {buffId}, level: {level}");
                return;
            }

            if (duration > 0) {
                duration = (int) TimeSpan.FromSeconds(1).TotalMilliseconds * duration;
            }

            long startTick = session.Field.FieldTick;
            if (all) {
                foreach (FieldPlayer player in session.Field.Players.Values) {
                    if (remove) {
                        player.Buffs.Remove(buffId, session.Player.ObjectId);
                    } else {
                        player.Buffs.AddBuff(session.Player, player, buffId, (short) level, startTick, stacks: stack, durationMs: duration);
                    }

                }
            } else if (!string.IsNullOrEmpty(target)) {
                FieldPlayer? player = session.Field.GetPlayers().Values
                    .FirstOrDefault(player => string.Equals(player.Value.Character.Name, target, StringComparison.OrdinalIgnoreCase));
                if (player is null) {
                    ctx.Console.Error.WriteLine($"Player {target} not found.");
                    return;
                }

                if (remove) {
                    player.Buffs.Remove(buffId, session.Player.ObjectId);
                    return;
                }
                player.Buffs.AddBuff(session.Player, player, buffId, (short) level, startTick, stacks: stack, durationMs: duration);
            } else {
                if (remove) {
                    session.Player.Buffs.Remove(buffId, session.Player.ObjectId);
                    return;
                }
                session.Player.Buffs.AddBuff(session.Player, session.Player, buffId, (short) level, startTick, stacks: stack, durationMs: duration);
            }

            ctx.ExitCode = 0;
        } catch (SystemException ex) {
            ctx.Console.Error.WriteLine(ex.Message);
            ctx.ExitCode = 1;
        }
    }
}
