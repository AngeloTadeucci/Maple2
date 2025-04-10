using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Model.Skill;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Collision;

namespace Maple2.Server.Game.Commands;

public class KillCommand : GameCommand {
    private const string NAME = "kill";
    private const string DESCRIPTION = "Kill NPCs/Mobs/Players";
    public const AdminPermissions RequiredPermission = AdminPermissions.GameMaster;

    public KillCommand(GameSession session) : base(RequiredPermission, NAME, DESCRIPTION) {
        AddCommand(new KillAllNpcCommand(session));
        AddCommand(new KillNearNpcCommand(session));
        AddCommand(new KillPlayerCommand(session));
    }

    private class KillAllNpcCommand : Command {
        private readonly GameSession session;

        public KillAllNpcCommand(GameSession session) : base("all", "Kill all NPCs in map.") {
            this.session = session;

            var npcOnly = new Option<bool>("npc", () => false, "Kill only npcs.");
            var mobOnly = new Option<bool>("mob", () => false, "Kill only mobs.");

            AddOption(npcOnly);
            AddOption(mobOnly);
            this.SetHandler<InvocationContext, bool, bool>(Handle, npcOnly, mobOnly);
        }

        private void Handle(InvocationContext ctx, bool npcOnly, bool mobOnly) {
            if (!session.SkillMetadata.TryGet(10000001, 1, out SkillMetadata? skill)) {
                ctx.Console.Out.WriteLine("Skill not found.");
                return;
            }

            if (npcOnly == mobOnly) {
                // Consider both to be true
                foreach (FieldNpc npc in session.Field.Npcs.Values) {
                    Kill(session, npc, skill);

                }
                foreach (FieldNpc npc in session.Field.Mobs.Values) {
                    Kill(session, npc, skill);
                }
                ctx.Console.Out.WriteLine("All NPCs and Mobs removed.");
                return;
            }

            if (npcOnly) {
                foreach (FieldNpc npc in session.Field.Npcs.Values) {
                    Kill(session, npc, skill);
                }
                ctx.Console.Out.WriteLine("All NPCs removed.");
            } else {
                foreach (FieldNpc npc in session.Field.Mobs.Values) {
                    Kill(session, npc, skill);
                }
                ctx.Console.Out.WriteLine("All Mobs removed.");
            }
        }
    }

    private class KillNearNpcCommand : Command {
        private readonly GameSession session;

        public KillNearNpcCommand(GameSession session) : base("near", "Kill mobs/npcs within a given range.") {
            this.session = session;

            var distance = new Option<int>(["--distance", "-d"], () => 150, "Radius to kill mobs/npcs.");

            AddOption(distance);
            this.SetHandler<InvocationContext, int>(Handle, distance);

        }

        private void Handle(InvocationContext ctx, int distance) {
            var rec = new Rectangle(new Vector2(session.Player.Position.X, session.Player.Position.Y), distance, distance, 0);
            var prism = new Prism(rec, session.Player.Position.Z, distance * 2);
            if (!session.SkillMetadata.TryGet(10000001, 1, out SkillMetadata? skill)) {
                ctx.Console.Out.WriteLine("Skill not found.");
                return;
            }
            foreach (FieldNpc npc in session.Field.Npcs.Values) {
                if (prism.Intersects(npc.Shape)) {
                    ctx.Console.Out.WriteLine($"Killing {npc.Value.Metadata.Name} - ObjectId: ({npc.ObjectId})");
                    Kill(session, npc, skill);
                }
            }

            foreach (FieldNpc npc in session.Field.Mobs.Values) {
                if (prism.Intersects(npc.Shape)) {
                    ctx.Console.Out.WriteLine($"Killing {npc.Value.Metadata.Name} - ObjectId: ({npc.ObjectId})");
                    Kill(session, npc, skill);
                }
            }
        }
    }

    private class KillPlayerCommand : Command {
        private readonly GameSession session;

        public KillPlayerCommand(GameSession session) : base("player", "Kill specific player in field.") {
            this.session = session;

            var name = new Argument<string>("name", "Name of player.");

            AddArgument(name);
            this.SetHandler<InvocationContext, string>(Handle, name);

        }

        private void Handle(InvocationContext ctx, string name) {
            if (string.IsNullOrEmpty(name)) {
                ctx.Console.Out.WriteLine("Name cannot be empty.");
                return;
            }
            FieldPlayer? player = session.Field.GetPlayers().Values
                .FirstOrDefault(player => string.Equals(player.Value.Character.Name, name, StringComparison.OrdinalIgnoreCase));

            if (player is null) {
                ctx.Console.Out.WriteLine($"Player {name} not found.");
                return;
            }

            if (player.DeathState != DeathState.Alive) {
                ctx.Console.Out.WriteLine($"Player {name} is already dead.");
                return;
            }

            player.ConsumeHp((int) player.Stats.Values[BasicAttribute.Health].Current);
        }
    }

    private static void Kill(GameSession session, FieldNpc npc, SkillMetadata skill) {
        var damageRecord = new DamageRecord(skill, skill.Data.Motions[0].Attacks[0]) {
            CasterId = session.Player.ObjectId,
            TargetUid = ((long) Random.Shared.Next(int.MinValue, int.MaxValue) << 32) | (uint) Random.Shared.Next(int.MinValue, int.MaxValue),
            OwnerId = session.Player.ObjectId,
            SkillId = skill.Id,
            Level = skill.Level,
            AttackPoint = 0,
            MotionPoint = 0,
            Position = npc.Position,
            Direction = session.Player.Rotation,
        };
        var target = new DamageRecordTarget() {
            ObjectId = npc.ObjectId,
            Position = session.Player.Position,
            Direction = session.Player.Rotation,
        };

        long damage = npc.Stats.Values[BasicAttribute.Health].Current;
        target.AddDamage(DamageType.Normal, damage);
        damageRecord.Targets.Add(target);
        npc.Stats.Values[BasicAttribute.Health].Add(-damage);
        session.Field.Broadcast(StatsPacket.Update(npc, BasicAttribute.Health));

        session.Field.Broadcast(SkillDamagePacket.Damage(damageRecord));
    }
}
