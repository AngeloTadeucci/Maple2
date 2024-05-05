using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Util;
using Maple2.Tools.Extensions;
using Serilog;

namespace Maple2.Server.Game.Manager;

public sealed class QuestManager {
    private const int BATCH_SIZE = 200;
    private readonly GameSession session;

    private readonly IDictionary<int, Quest> accountValues;
    private readonly IDictionary<int, Quest> characterValues;

    private readonly ILogger logger = Log.Logger.ForContext<QuestManager>();

    public QuestManager(GameSession session) {
        this.session = session;

        using GameStorage.Request db = session.GameStorage.Context();
        accountValues = db.GetQuests(session.AccountId);
        characterValues = db.GetQuests(session.CharacterId);
        Initialize(db);
    }

    public void Load() {
        session.Send(QuestPacket.StartLoad(0));

        foreach (ImmutableList<Quest> batch in accountValues.Values.Batch(BATCH_SIZE)) {
            session.Send(QuestPacket.LoadQuestStates(batch));
        }
        foreach (ImmutableList<Quest> batch in characterValues.Values.Batch(BATCH_SIZE)) {
            session.Send(QuestPacket.LoadQuestStates(batch));
        }
    }

    private void Initialize(GameStorage.Request db) {
        IEnumerable<QuestMetadata> quests = session.QuestMetadata.GetQuests();
        foreach (QuestMetadata metadata in quests) {
            if (!metadata.Basic.AutoStart ||
                metadata.Basic.Type == QuestType.FieldMission) {
                continue;
            }

            if (metadata.Basic.CompleteNpc != 0) {
                continue;
            }

            if (metadata.Basic.CompleteMaps != null && metadata.Basic.CompleteMaps.Length != 0) {
                continue;
            }

            if (!CanStart(metadata.Require)) {
                continue;
            }

            // TODO: figure out event handling for quests?
            if (metadata.Basic.EventTag != string.Empty) {
                continue;
            }

            var quest = new Quest(metadata) {
                Track = true,
                State = QuestState.Started,
                StartTime = DateTime.Now.ToEpochSeconds(),
            };

            for (int i = 0; i < metadata.Conditions.Length; i++) {
                quest.Conditions.Add(i, new Quest.Condition(metadata.Conditions[i]));
            }

            if (characterValues.ContainsKey(metadata.Id) || accountValues.ContainsKey(metadata.Id)) {
                continue;
            }
            quest = db.CreateQuest(session.CharacterId, quest);
            if (quest == null) {
                continue;
            }

            if (metadata.Basic.Account > 0) {
                accountValues.Add(metadata.Id, quest);
                continue;
            }
            Add(quest);
            session.Send(QuestPacket.Start(quest));
        }

    }

    /// <summary>
    /// Adds a quest to the quest manager.
    /// </summary>
    /// <param name="quest">Quest to add</param>
    private void Add(Quest quest) {
        if (quest.Metadata.Basic.Account > 0) {
            accountValues.Add(quest.Id, quest);
            return;
        }
        characterValues.Add(quest.Id, quest);
    }

    /// <summary>
    /// Starts a new quest (or prexisting if repeatable).
    /// </summary>
    public void Start(int questId, bool bypassRequirements = false) {
        if (characterValues.ContainsKey(questId) || accountValues.ContainsKey(questId)) {
            // TODO: see if you can start the quest again
            return;
        }

        if (!session.QuestMetadata.TryGet(questId, out QuestMetadata? metadata)) {
            return;
        }

        if (!bypassRequirements && !CanStart(metadata.Require)) {
            return;
        }

        var quest = new Quest(metadata) {
            Track = true,
            State = QuestState.Started,
            StartTime = DateTime.Now.ToEpochSeconds(),
        };

        for (int i = 0; i < metadata.Conditions.Length; i++) {
            quest.Conditions.Add(i, new Quest.Condition(metadata.Conditions[i]));
        }

        using GameStorage.Request db = session.GameStorage.Context();
        quest = db.CreateQuest(session.CharacterId, quest);
        if (quest == null) {
            logger.Error("Failed to create quest entry {questId}", metadata.Id);
            return;
        }
        Add(quest);

        // TODO: Confirm inventory can hold all the items.
        foreach (QuestMetadataReward.Item acceptReward in metadata.AcceptReward.EssentialItem) {
            Item? reward = session.Item.CreateItem(acceptReward.Id, acceptReward.Rarity, acceptReward.Amount);
            if (reward == null) {
                logger.Error("Failed to create quest reward {RewardId}", acceptReward.Id);
                continue;
            }
            if (!session.Item.Inventory.Add(reward, true)) {
                logger.Error("Failed to add quest reward {RewardId} to inventory", acceptReward.Id);
            }
        }

        session.ConditionUpdate(ConditionType.quest_accept, codeLong: quest.Id);
        session.Send(QuestPacket.Start(quest));
    }

    /// <summary>
    /// Updates all possible quests with the given condition type.
    /// </summary>
    /// <param name="type">Condition Type to update</param>
    /// <param name="counter">Condition value to progress by. Default is 1.</param>
    /// <param name="targetString">condition target parameter in string.</param>
    /// <param name="targetLong">condition target parameter in long.</param>
    /// <param name="codeString">condition code parameter in string.</param>
    /// <param name="codeLong">condition code parameter in long.</param>
    public void Update(ConditionType type, long counter = 1, string targetString = "", long targetLong = 0, string codeString = "", long codeLong = 0) {
        IEnumerable<Quest> quests = characterValues.Values.Where(quest => quest.State != QuestState.Completed)
            .Concat(accountValues.Values.Where(quest => quest.State != QuestState.Completed));
        foreach (Quest quest in quests) {
            // TODO: Not sure if ProgressMap really means that only progress counts in this map. It doesn't make sense for some quests.
            /*if (quest.Metadata.Basic.ProgressMaps != null && !quest.Metadata.Basic.ProgressMaps.Contains(session.Player.Value.Character.MapId)) {
                continue;
            }*/
            foreach (Quest.Condition condition in quest.Conditions.Values.Where(condition => condition.Metadata.Type == type)) {
                // Already meets the requirement and does not need to be updated
                if (condition.Counter >= condition.Metadata.Value) {
                    continue;
                }

                if (!condition.Metadata.Check(session, targetString, targetLong, codeString, codeLong)) {
                    continue;
                }

                condition.Counter = (int) Math.Min(condition.Metadata.Value, condition.Counter + counter);

                session.Send(QuestPacket.Update(quest));
            }
        }
    }

    /// <summary>
    /// Checks if player can start a quest.
    /// </summary>
    /// <param name="require">Metadata of the quest with the required values to start.</param>
    public bool CanStart(QuestMetadataRequire require) {
        if (require.Level > 0 && require.Level > session.Player.Value.Character.Level) {
            return false;
        }

        if (require.MaxLevel > 0 && require.MaxLevel < session.Player.Value.Character.Level) {
            return false;
        }

        if (require.Job.Length > 0 && !require.Job.Contains(session.Player.Value.Character.Job.Code())) {
            return false;
        }

        if (require.GearScore > session.Stats.Values.GearScore) {
            return false;
        }

        if (require.Achievement > 0 && !session.Achievement.TryGetAchievement(require.Achievement, out _)) {
            return false;
        }

        if (require.UnrequiredAchievement != (0, 0)
            && session.Achievement.TryGetAchievement(require.UnrequiredAchievement.Item1, out Achievement? achievement)
            && achievement.Grades.ContainsKey(require.UnrequiredAchievement.Item2)) {
            return false;
        }

        if (require.Quest.Length > 0) {
            foreach (int questId in require.Quest) {
                if (!TryGetQuest(questId, out Quest? requiredQuest) || requiredQuest.State != QuestState.Completed) {
                    return false;
                }
            }
        }

        return require.SelectableQuest.Length <= 0 || SelectableQuests(require.SelectableQuest);

    }

    /// <summary>
    /// Finds if any of the quests in the list are completed.
    /// </summary>
    /// <returns>Returns true if at least one quest has been completed by the player.</returns>
    private bool SelectableQuests(IEnumerable<int> questIds) {
        foreach (int questId in questIds) {
            if (TryGetQuest(questId, out Quest? quest) && quest.State == QuestState.Completed) {
                return true;
            }
        }

        return false;
    }

    public bool CanComplete(Quest quest) {
        return quest.State != QuestState.Completed && quest.Conditions
            .All(condition => condition.Value.Counter >= condition.Value.Metadata.Value);
    }

    /// <summary>
    /// Gives the player the rewards for completing the quest.
    /// </summary>
    public bool Complete(Quest quest, bool bypassConditions = false) {
        if (quest.State == QuestState.Completed) {
            return false;
        }

        if (!bypassConditions && !quest.Conditions.All(condition => condition.Value.Counter >= condition.Value.Metadata.Value)) {
            return false;
        }

        QuestMetadataReward reward = quest.Metadata.CompleteReward;
        if (reward.Exp > 0) {
            /*if (reward.RelativeExp != ExpType.none && session.TableMetadata.CommonExpTable.Entries.TryGetValue(reward.RelativeExp, out CommonExpTable.Entry? entry)) {
                entry.
            }
            long exp = reward.Exp;*/
            ExpType expType = quest.Metadata.Basic.Type switch {
                QuestType.EpicQuest => ExpType.epicQuest,
                QuestType.FieldMission => ExpType.mission,
                QuestType.AllianceQuest => ExpType.questSkyFortress,
                _ => ExpType.quest,
            };
            session.Exp.AddExp(expType, reward.Exp);
        }

        if (reward.Meso > 0) {
            session.Currency.Meso += reward.Meso;
        }

        if (reward.Treva > 0) {
            session.Currency[CurrencyType.Treva] += reward.Treva;
        }

        if (reward.Rue > 0) {
            session.Currency[CurrencyType.Rue] += reward.Rue;
        }

        foreach (QuestMetadataReward.Item entry in reward.EssentialItem) {
            Item? item = session.Item.CreateItem(entry.Id, entry.Rarity, entry.Amount);
            if (item != null) {
                session.Item.Inventory.Add(item, true);
            }
        }

        foreach (QuestMetadataReward.Item entry in reward.EssentialJobItem) {
            if (!session.ItemMetadata.TryGet(entry.Id, out ItemMetadata? metadata)) {
                continue;
            }
            if (metadata.Limit.JobLimits.Length > 0 && !metadata.Limit.JobLimits.Contains(session.Player.Value.Character.Job.Code())) {
                continue;
            }
            Item? item = session.Item.CreateItem(entry.Id, entry.Rarity, entry.Amount);
            if (item != null) {
                session.Item.Inventory.Add(item, true);
            }
        }

        // TODO: Guild rewards, mission points?

        session.ConditionUpdate(ConditionType.quest_clear_by_chapter, codeLong: quest.Metadata.Basic.ChapterId);
        session.ConditionUpdate(ConditionType.quest, codeLong: quest.Metadata.Id);
        session.ConditionUpdate(ConditionType.quest_clear, codeLong: quest.Metadata.Id);

        quest.EndTime = DateTime.Now.ToEpochSeconds();
        quest.State = QuestState.Completed;
        quest.CompletionCount++;
        session.Send(QuestPacket.Complete(quest));
        return true;
    }

    /// <summary>
    /// Gets available quests that the npc can give or can be completed.
    /// </summary>
    public SortedDictionary<int, QuestMetadata> GetAvailableQuests(int npcId) {
        var results = new SortedDictionary<int, QuestMetadata>();
        IEnumerable<QuestMetadata> allQuests = session.QuestMetadata.GetQuestsByNpc(npcId);

        // Get any new quests that can be started
        foreach (QuestMetadata metadata in allQuests) {
            if (TryGetQuest(metadata.Id, out Quest? quest) && quest.State == QuestState.Completed /* && repeatable */) {
                continue;
            }

            if (metadata.Basic.Disabled) {
                continue;
            }

            if (metadata.Basic.EventTag != string.Empty) {
                continue;
            }

            if (!session.Quest.CanStart(metadata.Require)) {
                continue;
            }

            if (!results.TryAdd(metadata.Id, metadata)) {
                // error
            }
        }

        // Get any quests that are in progress and npc is the completion npc
        foreach ((int id, Quest quest) in session.Quest.characterValues) {
            if (quest.Metadata.Basic.CompleteNpc == npcId && quest.State != QuestState.Completed) {
                if (!results.TryAdd(id, quest.Metadata)) {
                    // error
                }
            }
        }

        foreach ((int id, Quest quest) in session.Quest.accountValues) {
            if (quest.Metadata.Basic.CompleteNpc == npcId && quest.State != QuestState.Completed) {
                if (!results.TryAdd(id, quest.Metadata)) {
                    // error
                }
            }
        }

        return results;
    }

    public bool Remove(Quest quest) {
        using GameStorage.Request db = session.GameStorage.Context();
        if (quest.Metadata.Basic.Account > 0) {
            return accountValues.Remove(quest.Id) && db.DeleteQuest(session.AccountId, quest.Id);
        }
        return characterValues.Remove(quest.Id) && db.DeleteQuest(session.CharacterId, quest.Id);
    }

    public bool TryGetQuest(int questId, [NotNullWhen(true)] out Quest? quest) {
        return characterValues.TryGetValue(questId, out quest) || accountValues.TryGetValue(questId, out quest);
    }

    public void Save(GameStorage.Request db) {
        db.SaveQuests(session.AccountId, accountValues.Values);
        db.SaveQuests(session.CharacterId, characterValues.Values);
    }
}
