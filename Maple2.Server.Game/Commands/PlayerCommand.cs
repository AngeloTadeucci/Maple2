using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using Maple2.Database.Storage;
using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class PlayerCommand : GameCommand {
    private const string NAME = "player";
    private const string DESCRIPTION = "Player management.";
    public const AdminPermissions RequiredPermission = AdminPermissions.PlayerCommands;

    public PlayerCommand(GameSession session, AchievementMetadataStorage achievementMetadataStorage) : base(RequiredPermission, NAME, DESCRIPTION) {
        AddCommand(new LevelCommand(session));
        AddCommand(new PrestigeCommand(session));
        AddCommand(new ExpCommand(session));
        AddCommand(new JobCommand(session));
        AddCommand(new InfoCommand(session));
        AddCommand(new SkillPointCommand(session));
        AddCommand(new CurrencyCommand(session));
        AddCommand(new InventoryCommand(session));
        AddCommand(new TrophyCommand(session, achievementMetadataStorage));
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

                session.Player.Buffs.Clear();
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

    private class SkillPointCommand : Command {
        private readonly GameSession session;

        public SkillPointCommand(GameSession session) : base("skillpoint", "Add skill points to player.") {
            this.session = session;

            var points = new Argument<int>("points", "Skill points to add.");
            var rank = new Option<short>(["--rank", "-r"], () => 0, "Job rank to add points to. (0 for normal, 1 for awakening)");

            AddArgument(points);
            AddOption(rank);
            this.SetHandler<InvocationContext, int, short>(Handle, points, rank);
        }

        private void Handle(InvocationContext ctx, int points, short rank) {
            try {
                rank = (short) Math.Clamp((int) rank, 0, 1);
                session.Config.AddSkillPoint(SkillPointSource.Unknown, points, rank);
                ctx.ExitCode = 0;
            } catch (SystemException ex) {
                ctx.Console.Error.WriteLine(ex.Message);
                ctx.ExitCode = 1;
            }
        }
    }

    private class CurrencyCommand : Command {
        private readonly GameSession session;

        public CurrencyCommand(GameSession session) : base("currency", "Add currency to player.") {
            this.session = session;

            var currency = new Argument<string>("currency", "Type of currency to add: meso, meret, valortoken, treva, rue, havifruit, reversecoin, mentortoken, menteetoken, starpoint, mesotoken.");
            var amount = new Argument<long>("amount", "Amount of currency to add.");

            AddArgument(currency);
            AddArgument(amount);
            this.SetHandler<InvocationContext, string, long>(Handle, currency, amount);
        }

        private void Handle(InvocationContext ctx, string currency, long amount) {
            try {
                switch (currency.ToLower()) {
                    // Handling meso and meret separately because they are not in the CurrencyType enum.
                    case "meso":
                        session.Currency.Meso += amount;
                        break;
                    case "meret":
                        session.Currency.Meret += amount;
                        break;
                    case "gamemeret":
                        session.Currency.GameMeret += amount;
                        break;
                    default:
                        if (!Enum.TryParse(currency, true, out CurrencyType currencyType)) {
                            ctx.Console.Error.WriteLine($"Failed to parse currency type: {currency}");
                            ctx.ExitCode = 1;
                            return;
                        }
                        session.Currency[currencyType] += amount;
                        break;
                }
                ctx.ExitCode = 0;
            } catch (SystemException ex) {
                ctx.Console.Error.WriteLine(ex.Message);
                ctx.ExitCode = 1;
            }
        }
    }

    private class InventoryCommand : Command {
        private readonly GameSession session;

        public InventoryCommand(GameSession session) : base("inventory", "Manage player inventory.") {
            this.session = session;

            AddCommand(new ClearInventoryCommand(session));
            AddCommand(new SlotsInventoryCommand(session));
            AddCommand(new ExpandInventoryCommand(this.session));
        }

        private class ClearInventoryCommand : Command {
            private readonly GameSession session;

            public ClearInventoryCommand(GameSession session) : base("clear", "Clear player inventory.") {
                this.session = session;

                var invTab = new Argument<string>("tab", $"Inventory tab to clear. One of: {string.Join(", ", Enum.GetNames(typeof(InventoryType)))}");

                AddArgument(invTab);
                this.SetHandler<InvocationContext, string>(Handle, invTab);
            }

            private void Handle(InvocationContext ctx, string tab) {
                if (!Enum.TryParse(tab, true, out InventoryType inventoryType)) {
                    ctx.Console.Error.WriteLine($"Invalid inventory tab: {tab}. Must be one of: {string.Join(", ", Enum.GetNames(typeof(InventoryType)))}");
                    ctx.ExitCode = 1;
                    return;
                }

                session.Item.Inventory.Clear(inventoryType);
                ctx.Console.Out.WriteLine($"Cleared {inventoryType} inventory.");
                ctx.ExitCode = 0;
            }
        }

        private class SlotsInventoryCommand : Command {
            private readonly GameSession session;

            public SlotsInventoryCommand(GameSession session) : base("slots", "Get inventory slots information.") {
                this.session = session;

                var tabArg = new Argument<string>("tab", $"Inventory tab to get information for. Use 'all' for all tabs or one of: {string.Join(", ", Enum.GetNames(typeof(InventoryType)))}");
                AddArgument(tabArg);
                this.SetHandler<InvocationContext, string>(Handle, tabArg);
            }

            private void Handle(InvocationContext ctx, string tab) {
                try {
                    if (tab.Equals("all", StringComparison.OrdinalIgnoreCase)) {
                        foreach (InventoryType inventoryTab in Enum.GetValues(typeof(InventoryType))) {
                            DisplayInventoryInfo(ctx, inventoryTab);
                        }
                        ctx.ExitCode = 0;
                        return;
                    }

                    if (!Enum.TryParse(tab, true, out InventoryType inventoryType)) {
                        ctx.Console.Error.WriteLine($"Invalid inventory tab: {tab}. Must be one of: {string.Join(", ", Enum.GetNames(typeof(InventoryType)))}");
                        ctx.ExitCode = 1;
                        return;
                    }

                    DisplayInventoryInfo(ctx, inventoryType);
                    ctx.ExitCode = 0;
                } catch (SystemException ex) {
                    ctx.Console.Error.WriteLine(ex.Message);
                    ctx.ExitCode = 1;
                }
            }

            private void DisplayInventoryInfo(InvocationContext ctx, InventoryType inventoryType) {
                int totalSlots = session.Item.Inventory.TotalSlots(inventoryType);
                int freeSlots = session.Item.Inventory.FreeSlots(inventoryType);
                int usedSlots = totalSlots - freeSlots;

                ctx.Console.Out.WriteLine($"{inventoryType} Inventory: {usedSlots}/{totalSlots} slots used.");
            }
        }

        private class ExpandInventoryCommand : Command {
            private readonly GameSession session;

            public ExpandInventoryCommand(GameSession session) : base("expand", "Expand player inventory.") {
                this.session = session;

                var tab = new Argument<string>("tab", $"Inventory tab to expand. One of: {string.Join(", ", Enum.GetNames(typeof(InventoryType)))}");

                AddArgument(tab);
                this.SetHandler<InvocationContext, string>(Handle, tab);
            }

            private void Handle(InvocationContext ctx, string tab) {
                try {
                    if (!Enum.TryParse(tab, true, out InventoryType inventoryType)) {
                        ctx.Console.Error.WriteLine($"Invalid inventory tab: {tab}. Must be one of: {string.Join(", ", Enum.GetNames(typeof(InventoryType)))}");
                        ctx.ExitCode = 1;
                        return;
                    }

                    session.Item.Inventory.Expand(inventoryType);
                    ctx.Console.Out.WriteLine($"Expanded {inventoryType} inventory");
                    ctx.ExitCode = 0;
                } catch (SystemException ex) {
                    ctx.Console.Error.WriteLine(ex.Message);
                    ctx.ExitCode = 1;
                }
            }
        }
    }

    private class TrophyCommand : Command {
        private readonly GameSession session;
        private readonly AchievementMetadataStorage achievementMetadataStorage;

        public TrophyCommand(GameSession session, AchievementMetadataStorage achievementMetadataStorage)
            : base("trophy", "Add trophy to player.") {
            this.session = session;
            this.achievementMetadataStorage = achievementMetadataStorage;

            var trophyId = new Argument<string>("id", "ID of the trophy to add to the player. Use 'all' to unlock all trophies.");
            var gradeOption = new Option<short>([
                "--grade",
                "-g",
            ], () => 0, "Grade of the trophy to claim. Defaults to the maximum grade available.");

            AddArgument(trophyId);
            AddOption(gradeOption);
            this.SetHandler<InvocationContext, string, short>(Handle, trophyId, gradeOption);
        }

        private void Handle(InvocationContext ctx, string trophyId, short grade) {
            try {
                if (trophyId.Equals("all", StringComparison.CurrentCultureIgnoreCase)) {
                    UnlockAllTrophies(ctx);
                    return;
                }

                if (!int.TryParse(trophyId, out int trophyIdInt)) {
                    ctx.Console.Error.WriteLine($"Invalid trophy ID: {trophyId}. Use a valid ID or 'all'.");
                    ctx.ExitCode = 1;
                    return;
                }

                UnlockSingleTrophy(ctx, trophyIdInt, grade);
            } catch (Exception ex) {
                ctx.Console.Error.WriteLine($"Failed to process trophy command: {ex.Message}");
                ctx.ExitCode = 1;
            }
        }

        private void UnlockAllTrophies(InvocationContext ctx) {
            try {
                ICollection<AchievementMetadata> achievementMetadataCollection = achievementMetadataStorage.GetAll();
                foreach (AchievementMetadata metadata in achievementMetadataCollection) {
                    int trophyId = metadata.Id;
                    int maxGrade = metadata.Grades.Keys.Max();

                    UnlockTrophy(trophyId, metadata, maxGrade);
                }

                ctx.Console.Out.WriteLine("All trophies have been successfully unlocked to their maximum grades.");
                ctx.ExitCode = 0;
            } catch (Exception ex) {
                ctx.Console.Error.WriteLine($"Failed to unlock all trophies: {ex.Message}");
                ctx.ExitCode = 1;
            }
        }

        private void UnlockSingleTrophy(InvocationContext ctx, int trophyId, short grade) {
            try {
                if (!achievementMetadataStorage.TryGet(trophyId, out AchievementMetadata? achievementMetadata)) {
                    ctx.Console.Error.WriteLine($"Trophy ID {trophyId} is invalid or does not exist.");
                    ctx.ExitCode = 1;
                    return;
                }

                int maxGrade = achievementMetadata.Grades.Keys.Max();
                if (grade == 0) {
                    grade = (short) maxGrade;
                }

                if (grade < 1 || grade > maxGrade) {
                    ctx.Console.Error.WriteLine($"Invalid grade {grade} for Trophy ID {trophyId}. Available grades are between 1 and {maxGrade}.");
                    ctx.ExitCode = 1;
                    return;
                }

                UnlockTrophy(trophyId, achievementMetadata, maxGrade);
                ctx.ExitCode = 0;
            } catch (Exception ex) {
                ctx.Console.Error.WriteLine($"Failed to unlock trophy ID {trophyId} with grade {grade}: {ex.Message}");
                ctx.ExitCode = 1;
            }
        }

        private void UnlockTrophy(int trophyId, AchievementMetadata achievementMetadata, int maxGrade) {
            ConditionMetadata conditionMetadata = achievementMetadata.Grades[maxGrade].Condition;

            long targetValue = conditionMetadata.Value;
            long remainingProgress = targetValue;

            if (session.Achievement.TryGetAchievement(trophyId, out Achievement? achievement)) {
                remainingProgress = Math.Max(0, targetValue - achievement.Counter);
            }

            ConditionType conditionType = conditionMetadata.Type;

            if (conditionMetadata.Codes?.Integers is not null) {
                UpdateAchievementWithCodes(conditionMetadata.Codes.Integers, conditionMetadata.Target, remainingProgress, conditionType);
            } else if (conditionMetadata.Codes?.Strings is not null) {
                UpdateAchievementWithCodes(conditionMetadata.Codes.Strings, conditionMetadata.Target, remainingProgress, conditionType);
            } else if (conditionMetadata.Codes?.Range is not null) {
                UpdateAchievementWithCode(conditionMetadata.Codes.Range.Value.Min, conditionMetadata.Target, remainingProgress, conditionType);
            } else {
                UpdateAchievementWithoutCodes(conditionMetadata.Target, remainingProgress, conditionType);
            }
        }

        private void UpdateAchievementWithCodes<T>(IEnumerable<T> codes, ConditionMetadata.Parameters? target, long remainingProgress, ConditionType conditionType) where T : notnull {
            foreach (T code in codes) {
                if (target?.Integers is not null) {
                    foreach (int targetValue in target.Integers) {
                        UpdateAchievementWithCode(code, targetValue, remainingProgress, conditionType);
                    }
                } else if (target?.Strings is not null) {
                    foreach (string targetValue in target.Strings) {
                        UpdateAchievementWithCode(code, targetValue, remainingProgress, conditionType);
                    }
                } else if (target?.Range is not null) {
                    UpdateAchievementWithCode(code, target.Range.Value.Min, remainingProgress, conditionType);
                } else {
                    UpdateAchievementWithCode(code, null, remainingProgress, conditionType);
                }
            }
        }

        private void UpdateAchievementWithoutCodes(ConditionMetadata.Parameters? target, long remainingProgress, ConditionType conditionType) {
            if (target?.Integers is not null) {
                foreach (int targetValue in target.Integers) {
                    session.Achievement.Update(conditionType, count: remainingProgress, targetLong: targetValue);
                }
            } else if (target?.Strings is not null) {
                foreach (string targetValue in target.Strings) {
                    session.Achievement.Update(conditionType, count: remainingProgress, targetString: targetValue);
                }
            } else if (target?.Range is not null) {
                session.Achievement.Update(conditionType, count: remainingProgress, targetLong: target.Range.Value.Min);
            } else {
                session.Achievement.Update(conditionType, count: remainingProgress);
            }
        }

        private void UpdateAchievementWithCode(object? code, object? targetValue, long remainingProgress, ConditionType conditionType) {
            long codeLong = code switch {
                long l => l,
                int i => i,
                _ => 0,
            };

            session.Achievement.Update(conditionType, count: remainingProgress, codeLong: codeLong, codeString: code as string ?? "", targetLong: targetValue as int? ?? 0, targetString: targetValue as string ?? "");
        }
    }
}
