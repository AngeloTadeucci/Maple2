using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Event;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
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

    private ChangeJobMetadata? changeJobMetadata;

    private readonly ILogger logger = Log.Logger.ForContext<QuestManager>();

    public QuestManager(GameSession session) {
        this.session = session;

        using GameStorage.Request db = session.GameStorage.Context();
        accountValues = db.GetQuests(session.AccountId);
        characterValues = db.GetQuests(session.CharacterId);
        Initialize(db);
    }

    public void Load() {
        session.Send(QuestPacket.LoadExploration(session.Config.ExplorationProgress));

        foreach (ImmutableList<Quest> batch in accountValues.Values.Batch(BATCH_SIZE)) {
            session.Send(QuestPacket.LoadQuestStates(batch));
        }
        foreach (ImmutableList<Quest> batch in characterValues.Values.Batch(BATCH_SIZE)) {
            session.Send(QuestPacket.LoadQuestStates(batch));
        }
    }

    private void Initialize(GameStorage.Request db) {
        IEnumerable<QuestMetadata> quests = session.QuestMetadata.GetQuests();
        IList<QuestTag> events = session.FindEvent(GameEventType.QuestTag)
            .Where(gameEvent => gameEvent.Metadata.Data is QuestTag)
            .Select(gameEvent => (QuestTag) gameEvent.Metadata.Data)
            .ToList();
        foreach (QuestMetadata metadata in quests) {
            if (!metadata.Basic.AutoStart ||
                metadata.Basic.Type == QuestType.FieldMission) {
                continue;
            }

            // Event missions should only be started through the event.
            if (metadata.EventMissionType != QuestEventMissionType.none) {
                continue;
            }

            if (metadata.Basic.CompleteNpc != 0) {
                continue;
            }

            if (metadata.Basic.CompleteMaps != null && metadata.Basic.CompleteMaps.Length != 0) {
                continue;
            }

            if (!CanStart(metadata)) {
                continue;
            }

            if (characterValues.ContainsKey(metadata.Id) || accountValues.ContainsKey(metadata.Id)) {
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

            long ownerId = metadata.Basic.Account > 0 ? session.AccountId : session.CharacterId;
            quest = db.CreateQuest(ownerId, quest);
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
    public QuestError Start(int questId, bool bypassRequirements = false) {
        if (characterValues.ContainsKey(questId) || accountValues.ContainsKey(questId)) {
            // TODO: see if you can start the quest again
            return QuestError.s_quest_error_accept_fail;
        }

        if (!session.QuestMetadata.TryGet(questId, out QuestMetadata? metadata)) {
            return QuestError.s_quest_error_accept_fail;
        }

        if (!bypassRequirements && !CanStart(metadata)) {
            return QuestError.s_quest_error_accept_fail;
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
        long ownerId = metadata.Basic.Account > 0 ? session.AccountId : session.CharacterId;
        quest = db.CreateQuest(ownerId, quest);
        if (quest == null) {
            logger.Error("Failed to create quest entry {questId}", metadata.Id);
            return QuestError.s_quest_error_accept_fail;
        }
        Add(quest);

        // TODO: Confirm inventory can hold all the items.
        foreach (QuestMetadataReward.Item acceptReward in metadata.AcceptReward.EssentialItem) {
            Item? reward = session.Field.ItemDrop.CreateItem(acceptReward.Id, acceptReward.Rarity, acceptReward.Amount);
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
        if (quest.Metadata.SummonPortal != null) {
            SummonPortal(quest);
        }
        return QuestError.none;
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
            .Concat(accountValues.Values.Where(quest => quest.State != QuestState.Completed))
            .Where(x => x.Conditions.Values.Any(quest => quest.Metadata.Type == type));
        foreach (Quest quest in quests) {
            // TODO: Not sure if ProgressMap really means that only progress counts in this map. It doesn't make sense for some quests.
            // Testing only on FieldMission for now.
            if (quest.Metadata.Basic is { Type: QuestType.FieldMission, ProgressMaps: not null } && session.Field is not null) {
                switch (session.Field.Metadata.Property.ExploreType) {
                    case 1 when !quest.Metadata.Basic.ProgressMaps.Contains(session.Player.Value.Character.MapId):
                    case 2 when !quest.Metadata.Basic.ProgressMaps.Any(x => session.Player.Value.Character.ReturnMaps.Contains(x)):
                        continue;
                }
            }

            if (quest.Metadata.Basic.Type == QuestType.MentoringMission && quest.Metadata.Mentoring != null) {
                // if the required opening day hasn't passed yet, skip
                if ((DateTime.Now - quest.StartTime.FromEpochSeconds()).TotalDays < quest.Metadata.Mentoring.OpeningDay) {
                    continue;
                }
            }

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
                if (quest.Metadata.Basic.Type == QuestType.FieldMission &&
                    CanComplete(quest)) {
                    Complete(quest);
                }
            }
        }
    }

    /// <summary>
    /// Checks if player can start a quest.
    /// </summary>
    /// <param name="metadata">Metadata of the quest.</param>
    public bool CanStart(QuestMetadata metadata) {
        if (metadata.Basic.Disabled) {
            return false;
        }

        if (metadata.Basic.EventTag != string.Empty &&
            !session.FindEvent(GameEventType.QuestTag).Any(gameEvent => gameEvent.Metadata.Data is QuestTag tag && tag.Tag == metadata.Basic.EventTag)) {
            return false;
        }

        QuestMetadataRequire require = metadata.Require;
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

        List<Item> rewards = [];
        foreach (QuestMetadataReward.Item entry in reward.EssentialItem) {
            Item? item = session.Field.ItemDrop.CreateItem(entry.Id, entry.Rarity, entry.Amount);
            if (item is null) {
                continue;
            }

            rewards.Add(item);
        }

        foreach (QuestMetadataReward.Item entry in reward.EssentialJobItem) {
            if (!session.ItemMetadata.TryGet(entry.Id, out ItemMetadata? metadata)) {
                continue;
            }

            if (metadata.Limit.JobRecommends.Length > 0 && !metadata.Limit.JobRecommends.Contains(JobCode.None) && !metadata.Limit.JobRecommends.Contains(session.Player.Value.Character.Job.Code())) {
                continue;
            }

            Item? item = session.Field.ItemDrop.CreateItem(entry.Id, entry.Rarity, entry.Amount);
            if (item is null) {
                continue;
            }

            rewards.Add(item);
        }

        if (!session.Item.Inventory.CanAdd(rewards) && !Constant.MailQuestItems) {
            session.Send(ItemInventoryPacket.Error(ItemInventoryError.s_err_inventory));
            return false;
        }

        if (reward.Exp > 0) {
            session.Exp.AddExp(reward.Exp);
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

        foreach (Item item in rewards) {
            if (!session.Item.Inventory.Add(item, true)) {
                session.Item.MailItem(item);
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
        TryJobAdvance(quest.Id);
        CompleteChapter(quest.Id);
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

            if (!session.Quest.CanStart(metadata)) {
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

    public void Expired(IList<int> questIds) {
        foreach (int questId in questIds) {
            if (!session.Quest.TryGetQuest(questId, out Quest? quest)) {
                continue;
            }

            session.Quest.Remove(quest);
        }
        session.Send(QuestPacket.Expired(questIds));
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

    private void TryJobAdvance(int questId) {
        if (changeJobMetadata == null) {
            if (!session.TableMetadata.ChangeJobTable.Entries.TryGetValue(session.Player.Value.Character.Job, out ChangeJobMetadata? metadata)) {
                return;
            }
            changeJobMetadata = metadata;
        }

        if (changeJobMetadata.EndQuestId != questId) {
            return;
        }

        session.Player.Value.Character.Job = changeJobMetadata.ChangeJob;
        session.Config.Skill.SkillInfo.SetJob(changeJobMetadata.ChangeJob);
        session.Stats.Refresh();
        session.Field?.Broadcast(JobPacket.Advance(session.Player, session.Config.Skill.SkillInfo));
    }

    private void CompleteChapter(int questId, bool ignoreItemRewards = false) {
        IEnumerable<ChapterBookTable.Entry> entries = session.TableMetadata.ChapterBookTable.Entries.Values.Where(entry => entry.EndQuestId == questId).ToList();
        if (!entries.Any()) {
            return;
        }

        foreach (ChapterBookTable.Entry entry in entries) {
            if (!TryGetQuest(entry.BeginQuestId, out Quest? quest) || quest.State != QuestState.Completed) {
                continue;
            }

            if (!ignoreItemRewards) {
                foreach (ItemComponent itemComponent in entry.Items) {
                    Item? item = session.Field.ItemDrop.CreateItem(itemComponent.ItemId, itemComponent.Rarity, itemComponent.Amount);
                    if (item == null) {
                        continue;
                    }

                    if (!session.Item.Inventory.Add(item, true)) {
                        session.Item.MailItem(item);
                    }
                }
            }

            foreach (ChapterBookTable.Entry.SkillPoint point in entry.SkillPoints) {
                session.Config.AddSkillPoint(SkillPointSource.Chapter, point.Amount, point.Rank);
            }

            if (entry.StatPoints > 0) {
                session.Config.AddStatPoint(AttributePointSource.Quest, entry.StatPoints);
            }
        }
    }

    public void CompleteFieldMission(int mission) {
        if (!session.TableMetadata.FieldMissionTable.Entries.TryGetValue(mission, out FieldMissionTable.Entry? metadata)) {
            return;
        }

        int missionCompleteCount = characterValues.Values.Count(quest => quest.Metadata.Basic.Type == QuestType.FieldMission && quest.State == QuestState.Completed);
        missionCompleteCount += accountValues.Values.Count(quest => quest.Metadata.Basic.Type == QuestType.FieldMission && quest.State == QuestState.Completed);

        if (metadata.MissionCount > missionCompleteCount) {
            return;
        }

        session.Config.ExplorationProgress = metadata.MissionCount;
        session.Send(QuestPacket.UpdateExploration(session.Config.ExplorationProgress));

        if (metadata.Item != null) {
            Item? item = session.Field.ItemDrop.CreateItem(metadata.Item.ItemId, metadata.Item.Rarity, metadata.Item.Rarity);
            if (item != null) {
                if (!session.Item.Inventory.Add(item, true)) {
                    session.Item.MailItem(item);
                }
            }
        }

        if (metadata.StatPoints > 0) {
            session.Config.AddStatPoint(AttributePointSource.Exploration, metadata.StatPoints);
        }
    }

    public void LevelPotion(int level, int lastQuest = 0) {
        List<IGrouping<int, QuestMetadata>> chapterQuestsGroups = session.QuestMetadata.GetQuestsByType(QuestType.EpicQuest)
            .Where(q => q.Require.Level <= level)
            .Where(q => q.Basic.Disabled == false)
            .GroupBy(q => q.Basic.ChapterId)
            .ToList();

        using GameStorage.Request db = session.GameStorage.Context();
        bool lastQuestFound = false;
        foreach (IGrouping<int, QuestMetadata> chapterQuests in chapterQuestsGroups.OrderBy(q => q.Key)) {
            if (lastQuestFound) {
                break;
            }
            foreach (QuestMetadata metadata in chapterQuests.OrderBy(q => q.Id)) {
                if (TryGetQuest(metadata.Id, out Quest? quest)) {
                    if (quest.State == QuestState.Completed) {
                        continue;
                    }

                    foreach (Quest.Condition condition in quest.Conditions.Values) {
                        condition.Counter = (int) condition.Metadata.Value;
                    }
                    quest.State = QuestState.Completed;
                    quest.CompletionCount++;
                    quest.EndTime = DateTime.Now.ToEpochSeconds();
                    continue;
                }

                if (metadata.Require.Job.Length > 0 && !metadata.Require.Job.Contains(session.Player.Value.Character.Job.Code())) {
                    continue;
                }

                var epicQuest = new Quest(metadata) {
                    State = QuestState.Completed,
                    StartTime = DateTime.Now.ToEpochSeconds(),
                    EndTime = DateTime.Now.ToEpochSeconds() + 1,
                    CompletionCount = 1,
                    Track = true,
                };

                for (int i = 0; i < metadata.Conditions.Length; i++) {
                    epicQuest.Conditions.Add(i, new Quest.Condition(metadata.Conditions[i]));
                    epicQuest.Conditions[i].Counter = (int) epicQuest.Conditions[i].Metadata.Value;
                }

                long ownerId = metadata.Basic.Account > 0 ? session.AccountId : session.CharacterId;
                epicQuest = db.CreateQuest(ownerId, epicQuest);
                if (epicQuest == null) {
                    logger.Error("Failed to create quest entry {questId}", metadata.Id);
                    continue;
                }
                Add(epicQuest);
                TryJobAdvance(epicQuest.Id);
                CompleteChapter(epicQuest.Id, true);

                if (epicQuest.Id == lastQuest) {
                    lastQuestFound = true;
                    break;
                }
            }
        }

        //TODO: Start and load fame quests
        Load();
    }

    private void SummonPortal(Quest quest) {
        if (session.NpcScript?.Npc == null) {
            logger.Warning("Cannot summon quest portal for quest {QuestId}: No NPC script context", quest.Id);
            return;
        }

        FieldPortal portal = session.Field.SpawnPortal(quest.Metadata.SummonPortal!, session.NpcScript.Npc, session.Player);
        session.Send(PortalPacket.Add(portal));
        session.Send(QuestPacket.SummonPortal(session.NpcScript.Npc.ObjectId, portal.Value.Id, portal.StartTick));
    }

    /// <summary>
    /// Only used for debugging purposes. Completes all quests in the chapter silently.
    /// </summary>
    /// <param name="chapterId"></param>
    public void DebugCompleteChapter(int chapterId) {
        using GameStorage.Request db = session.GameStorage.Context();
        IEnumerable<int> questIds = session.QuestMetadata.GetQuestsByChapter(chapterId).Select(q => q.Id);
        foreach (int questId in questIds) {
            if (TryGetQuest(questId, out Quest? quest)) {
                if (quest.State == QuestState.Completed) {
                    continue;
                }

                quest.State = QuestState.Completed;
                quest.CompletionCount++;
                quest.EndTime = DateTime.Now.ToEpochSeconds();
            } else {
                if (!session.QuestMetadata.TryGet(questId, out QuestMetadata? metadata)) {
                    continue;
                }
                var newQuest = new Quest(metadata) {
                    State = QuestState.Completed,
                    StartTime = DateTime.Now.ToEpochSeconds(),
                    EndTime = DateTime.Now.ToEpochSeconds() + 1,
                    CompletionCount = 1,
                    Track = true,
                };

                for (int i = 0; i < newQuest.Metadata.Conditions.Length; i++) {
                    newQuest.Conditions.Add(i, new Quest.Condition(newQuest.Metadata.Conditions[i]));
                    newQuest.Conditions[i].Counter = (int) newQuest.Conditions[i].Metadata.Value;
                }

                long ownerId = newQuest.Metadata.Basic.Account > 0 ? session.AccountId : session.CharacterId;
                newQuest = db.CreateQuest(ownerId, newQuest);
                if (newQuest == null) {
                    logger.Error("Failed to create quest entry {questId}", metadata.Id);
                    continue;
                }
                Add(newQuest);
            }
        }

        Load();
    }

    public void Save(GameStorage.Request db) {
        db.SaveQuests(session.AccountId, accountValues.Values);
        db.SaveQuests(session.CharacterId, characterValues.Values);
    }
}
