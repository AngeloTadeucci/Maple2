using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class PlayerCommand : Command {
    private const string NAME = "player";
    private const string DESCRIPTION = "Player management.";

    public PlayerCommand(GameSession session) : base(NAME, DESCRIPTION) {
        AddCommand(new LevelCommand(session));
        AddCommand(new PrestigeCommand(session));
        AddCommand(new ExpCommand(session));
        AddCommand(new JobCommand(session));
        AddCommand(new InfoCommand(session));
    }

    private class LevelCommand : Command {
        private readonly GameSession session;

        public LevelCommand(GameSession session) : base("level", "Set player level.") {
            this.session = session;

            var level = new Argument<short>("level", "Level of the player.");

            AddArgument(level);
            this.SetHandler<InvocationContext, short>(Handle, level);
        }

        private void Handle(InvocationContext ctx, short level) {
            try {
                if (level is < 1 or > Constant.characterMaxLevel) {
                    ctx.Console.Error.WriteLine($"Invalid level: {level}. Must be between 1 and {Constant.characterMaxLevel}.");
                    return;
                }

                session.Player.Value.Character.Level = level;
                session.Field?.Broadcast(LevelUpPacket.LevelUp(session.Player));
                session.Stats.Refresh();

                session.ConditionUpdate(ConditionType.level, targetLong: level);

                session.PlayerInfo.SendUpdate(new PlayerUpdateRequest {
                    AccountId = session.AccountId,
                    CharacterId = session.CharacterId,
                    Level = level,
                    Async = true,
                });

                ctx.ExitCode = 0;
            } catch (SystemException ex) {
                ctx.Console.Error.WriteLine(ex.Message);
                ctx.ExitCode = 1;
            }
        }
    }

    private class ExpCommand : Command {
        private readonly GameSession session;

        public ExpCommand(GameSession session) : base("exp", "Add player experience.") {
            this.session = session;

            var exp = new Argument<long>("exp", "Exp amount.");

            AddArgument(exp);
            this.SetHandler<InvocationContext, long>(Handle, exp);
        }

        private void Handle(InvocationContext ctx, long exp) {
            try {
                session.Exp.AddExp(ExpType.none, exp);

                ctx.ExitCode = 0;
            } catch (SystemException ex) {
                ctx.Console.Error.WriteLine(ex.Message);
                ctx.ExitCode = 1;
            }
        }
    }

    private class PrestigeCommand : Command {
        private readonly GameSession session;

        public PrestigeCommand(GameSession session) : base("prestige", "Sets prestige level") {
            this.session = session;

            var level = new Argument<int>("level", "Prestige level of the player.");
            AddArgument(level);
            this.SetHandler<InvocationContext, int>(Handle, level);
        }

        private void Handle(InvocationContext ctx, int level) {
            try {
                if (level is < 1 or > Constant.AdventureLevelLimit) {
                    ctx.Console.Error.WriteLine($"Invalid level: {level}. Must be between 1 and {Constant.AdventureLevelLimit}.");
                    return;
                }

                int currentLevel = session.Exp.PrestigeLevel;
                session.Exp.PrestigeLevelUp(level - currentLevel);

                ctx.ExitCode = 0;
            } catch (SystemException ex) {
                ctx.Console.Error.WriteLine(ex.Message);
                ctx.ExitCode = 1;
            }
        }
    }

    private class JobCommand : Command {
        private readonly GameSession session;

        public JobCommand(GameSession session) : base("job", "Set player job.") {
            this.session = session;

            var jobCode = new Argument<JobCode>("jobcode", "JobCode of the player.");
            var awakening = new Option<bool>("awakening", "Awakening job advancement.");

            AddArgument(jobCode);
            AddOption(awakening);
            this.SetHandler<InvocationContext, JobCode, bool>(Handle, jobCode, awakening);
        }

        private void Handle(InvocationContext ctx, JobCode jobCode, bool awakening) {
            try {
                Job job = jobCode switch {
                    JobCode.Newbie => Job.Newbie,
                    JobCode.Knight => awakening ? Job.KnightII : Job.Knight,
                    JobCode.Berserker => awakening ? Job.BerserkerII : Job.Berserker,
                    JobCode.Wizard => awakening ? Job.WizardII : Job.Wizard,
                    JobCode.Priest => awakening ? Job.PriestII : Job.Priest,
                    JobCode.Archer => awakening ? Job.ArcherII : Job.Archer,
                    JobCode.HeavyGunner => awakening ? Job.HeavyGunnerII : Job.HeavyGunner,
                    JobCode.Thief => awakening ? Job.ThiefII : Job.Thief,
                    JobCode.Assassin => awakening ? Job.AssassinII : Job.Assassin,
                    JobCode.RuneBlader => awakening ? Job.RuneBladerII : Job.RuneBlader,
                    JobCode.Striker => awakening ? Job.StrikerII : Job.Striker,
                    JobCode.SoulBinder => awakening ? Job.SoulBinderII : Job.SoulBinder,
                    _ => throw new ArgumentException($"Invalid JobCode: {jobCode}")
                };

                Job currentJob = session.Player.Value.Character.Job;
                if (currentJob.Code() != job.Code()) {
                    foreach (SkillTab skillTab in session.Config.Skill.SkillBook.SkillTabs) {
                        skillTab.Skills.Clear();
                    }
                } else if (job < currentJob) {
                    foreach (SkillTab skillTab in session.Config.Skill.SkillBook.SkillTabs) {
                        foreach (int skillId in skillTab.Skills.Keys.ToList()) {
                            if (session.Config.Skill.SkillInfo.GetMainSkill(skillId, SkillRank.Awakening) != null) {
                                skillTab.Skills.Remove(skillId);
                            }
                        }
                    }
                    session.Config.Skill.ResetSkills(SkillRank.Awakening);
                }

                session.Player.Value.Character.Job = job;
                session.Config.Skill.SkillInfo.SetJob(job);

                session.Player.Buffs.Buffs.Clear();
                session.Player.Buffs.Initialize();
                session.Player.Buffs.LoadFieldBuffs();
                session.Stats.Refresh();
                session.Field?.Broadcast(JobPacket.Advance(session.Player, session.Config.Skill.SkillInfo));
                ctx.ExitCode = 0;
            } catch (SystemException ex) {
                ctx.Console.Error.WriteLine(ex.Message);
                ctx.ExitCode = 1;
            }
        }
    }

    private class InfoCommand : Command {
        private readonly GameSession session;

        public InfoCommand(GameSession session) : base("info", "Prints player info.") {
            this.session = session;

            this.SetHandler<InvocationContext>(Handle);
        }

        private void Handle(InvocationContext ctx) {
            ctx.Console.Out.WriteLine($"Player: {session.Player.ObjectId} ({session.PlayerName})");
            ctx.Console.Out.WriteLine($"  Position: {session.Player.Position}");
            ctx.Console.Out.WriteLine($"  Rotation: {session.Player.Rotation}");
        }
    }

    private class DebugAnimationCommand : Command {
        private readonly GameSession session;

        public DebugAnimationCommand(GameSession session) : base("debuganims", "Prints player animation info.") {
            this.session = session;

            var enable = new Argument<bool?>("enable", () => true, "Enables & disables debug messages. Prints all animation state if true.");

            AddArgument(enable);

            this.SetHandler<InvocationContext, bool?>(Handle, enable);
        }

        private void Handle(InvocationContext ctx, bool? enabled) {
            session.Player.AnimationState.DebugPrintAnimations = enabled ?? true;

            string message = enabled ?? true ? "Enabled" : "Disabled";
            ctx.Console.Out.WriteLine($"{message} animation debug info printing");
        }
    }

    private class DebugSkillsCommand : Command {
        private readonly GameSession session;

        public DebugSkillsCommand(GameSession session) : base("debugskills", "Prints player skill packet info.") {
            this.session = session;

            var enable = new Argument<bool?>("enable", () => true, "Enables & disables debug messages. Prints all skill cast packets if true.");

            AddArgument(enable);

            this.SetHandler<InvocationContext, bool?>(Handle, enable);
        }

        private void Handle(InvocationContext ctx, bool? enabled) {
            session.Player.DebugSkills = enabled ?? true;

            string message = enabled ?? true ? "Enabled" : "Disabled";
            ctx.Console.Out.WriteLine($"{message} skill cast packet debug info printing");
        }
    }
}
