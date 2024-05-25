using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;
using Serilog;

namespace Maple2.Server.Game.Manager;

public sealed class NpcScriptManager {
    private readonly GameSession session;

    public readonly FieldNpc Npc;
    private readonly ScriptMetadata? metadata;
    private readonly Dictionary<int, ScriptConditionMetadata>? scriptConditions;
    public JobConditionMetadata? JobCondition;
    public SortedDictionary<int, QuestMetadata> Quests = new();

    public NpcTalkType TalkType;
    public NpcTalkButton Button;

    public ScriptState? State { get; set; }
    public int Index { get; private set; } = 0;
    public int QuestId { get; private set; } = 0;
    private readonly ILogger logger = Log.Logger.ForContext<NpcScriptManager>();

    public NpcScriptManager(GameSession session, FieldNpc npc, ScriptMetadata? metadata) {
        this.session = session;
        Npc = npc;
        this.metadata = metadata;
        if (session.ServerTableMetadata.ScriptConditionTable.Entries.TryGetValue(Npc.Value.Id, out Dictionary<int, ScriptConditionMetadata>? scriptConditionMetadata)) {
            scriptConditions = scriptConditionMetadata;
        }
        if (this.metadata?.Type == ScriptType.Quest) {
            TalkType = NpcTalkType.Quest;
            State = GetQuestScriptState(this.metadata);
        }
    }

    public bool BeginNpcTalk() {
        if (!SetInitialScript()) {
            return false;
        }
        NpcRespond();

        return true;
    }

    public bool BeginQuest() {
        TalkType = NpcTalkType.Quest;
        return QuestRespond();
    }

    private bool QuestRespond() {
        State = GetQuestScriptState(metadata);
        if (State == null || State.Id == 0) {
            session.Send(NpcTalkPacket.Close());
            return false;
        }
        Button = GetButton();

        var dialogue = new NpcDialogue(State.Id, Index, Button);
        session.Send(NpcTalkPacket.Continue(TalkType, dialogue, metadata!.Id));
        return true;
    }

    public void EnterDialog() {
        // Basic shops are treated slightly different
        if (Npc.Value.Metadata.Basic.Kind is 1 or > 10 and < 20) {
            TalkType = NpcTalkType.Talk;
            State = null;
            Button = GetButton();
            return;
        }

        EnterTalk();
    }

    public void EnterTalk() {
        ScriptState? scriptState = GetFirstScriptState();
        if (scriptState == null) {
            return;
        }
        TalkType = scriptState.Type == ScriptStateType.Script ? NpcTalkType.Talk : NpcTalkType.Dialog;

        State = scriptState;
        Button = GetButton();
    }

    private bool SetInitialScript() {
        if (metadata?.Type == ScriptType.Quest) {
            TalkType = NpcTalkType.Quest;
            return true;
        }

        Quests = session.Quest.GetAvailableQuests(Npc.Value.Id);
        ScriptState? scriptState = GetFirstScriptState();
        ScriptState? selectState = GetSelectScriptState();
        ScriptState? questState = GetFirstQuestScriptState();

        int options = 0;
        if (Npc.Value.Metadata.Basic.ShopId > 0) {
            TalkType |= NpcTalkType.Dialog;
            options++;
        }

        if (Quests.Count > 0) {
            TalkType |= NpcTalkType.Quest;
            options++;
        }


        if (scriptState?.Type == ScriptStateType.Job) {
            // Job script only counts as an additional option if quests are present.
            if (TalkType.HasFlag(NpcTalkType.Quest)) {
                options++;
            }
            TalkType |= NpcTalkType.Dialog;
        } else if (scriptState != null) {
            TalkType |= NpcTalkType.Talk;
            options++;
        }

        if (options > 1 && selectState != null) {
            TalkType |= NpcTalkType.Select;
        }

        switch (Npc.Value.Metadata.Basic.Kind) {
            case >= 30 and < 40: // Beauty
            case 2: // Storage
            case 86: // TODO: BlackMarket
            case 88: // TODO: Birthday
            case 501:
                TalkType |= NpcTalkType.Dialog;
                break;
            case >= 100 and <= 104: // Sky Fortress
            case >= 105 and <= 107: // Kritias
            case 108: // Humanitas
                TalkType = NpcTalkType.Dialog;
                State = selectState;
                return true;
        }

        // Determine which script to use.
        if (TalkType.HasFlag(NpcTalkType.Select)) {
            State = selectState;
            return true;
        }

        if (TalkType.HasFlag(NpcTalkType.Quest)) {
            if (questState == null) {
                return false;
            }
            State = questState;
            TalkType = NpcTalkType.Quest;
            return true;
        }

        if (scriptState == null && selectState == null) {
            return false;
        }
        State = scriptState ?? selectState;
        return true;
    }

    private ScriptState? GetFirstScriptState() {
        if (metadata == null) {
            return null;
        }
        // Check if player meets the requirements for the job script.
        ScriptState? jobScriptState = metadata.States.Values.FirstOrDefault(state => state.Type == ScriptStateType.Job);
        if (jobScriptState != null) {
            if (!session.ServerTableMetadata.JobConditionTable.Entries.TryGetValue(Npc.Value.Id, out JobConditionMetadata? jobCondition)) {
                return jobScriptState;
            }
            this.JobCondition = jobCondition;
            if (MeetsJobCondition()) {
                return jobScriptState;
            }
        }


        IList<ScriptState> scriptStates = new List<ScriptState>();
        // Check if player meets the requirements for each pick script.
        foreach (ScriptState scriptState in metadata.States.Values.Where(state => state.Pick)) {
            if (scriptConditions == null) {
                scriptStates.Add(scriptState);
                continue;
            }

            if (scriptState.JobCondition != null &&
                scriptState.JobCondition != JobCode.None &&
                scriptState.JobCondition != session.Player.Value.Character.Job.Code()) {
                continue;
            }

            if (!scriptConditions.TryGetValue(scriptState.Id, out ScriptConditionMetadata? scriptCondition)) {
                scriptStates.Add(scriptState);
                continue;
            }

            if (MeetsScriptCondition(scriptCondition)) {
                scriptStates.Add(scriptState);
            }
        }

        return scriptStates.Count == 0 ? null : scriptStates[Random.Shared.Next(scriptStates.Count)];
    }

    public bool Continue(int pick) {
        ScriptState? nextState = NextState(pick);
        if (nextState == null) {
            session.Send(NpcTalkPacket.Close());
            if (State?.Type == ScriptStateType.Job) {
                PerformJobScript();
            }
            return false;
        }

        int questId = 0;
        if (metadata!.Type == ScriptType.Quest) {
            questId = metadata.Id;
        }
        if ((nextState != State && !metadata.States.ContainsKey(nextState.Id))) {
            session.Send(NpcTalkPacket.Close());
            return false;
        }

        if (nextState == State) {
            Index++;
        } else {
            Index = 0;
        }

        State = nextState;
        Button = GetButton();

        var dialogue = new NpcDialogue(State.Id, Index, Button);
        ProcessScriptFunction();
        session.Send(NpcTalkPacket.Continue(TalkType, dialogue, questId));
        return true;
    }

    private ScriptState? NextState(int pick) {
        if (State == null) {
            return null;
        }

        CinematicContent? content = State.Contents.ElementAtOrDefault(Index);
        if (content == null) {
            return null;
        }

        if (content.Distractors.Length > 0) {
            CinematicDistractor? distractor = content.Distractors.ElementAtOrDefault(pick);
            if (distractor == null) {
                return null;
            }

            IList<ScriptState> goToScripts = new List<ScriptState>();
            foreach (int goToScriptId in distractor.Goto) {
                if (!metadata!.States.TryGetValue(goToScriptId, out ScriptState? goToScript)) {
                    continue;
                }

                if (scriptConditions == null || !scriptConditions.TryGetValue(goToScript.Id, out ScriptConditionMetadata? scriptConditionMetadata)) {
                    goToScripts.Add(goToScript);
                    continue;
                }

                if (MeetsScriptCondition(scriptConditionMetadata)) {
                    goToScripts.Add(goToScript);
                }
            }

            if (goToScripts.Count > 0) {
                return goToScripts[Random.Shared.Next(goToScripts.Count)];
            }

            IList<ScriptState> goToFailScripts = new List<ScriptState>();
            foreach (int goToFailScriptId in distractor.GotoFail) {
                if (!metadata!.States.TryGetValue(goToFailScriptId, out ScriptState? goToFailScript)) {
                    continue;
                }

                if (scriptConditions == null || !scriptConditions.TryGetValue(goToFailScript.Id, out ScriptConditionMetadata? scriptConditionMetadata)) {
                    goToFailScripts.Add(goToFailScript);
                    continue;
                }

                if (MeetsScriptCondition(scriptConditionMetadata)) {
                    goToFailScripts.Add(goToFailScript);
                }
            }

            if (goToFailScripts.Count > 0) {
                return goToFailScripts[Random.Shared.Next(goToFailScripts.Count)];
            }
        }

        if (Index >= State.Contents.Length - 1) {
            return null;
        }

        return State;
    }

    private bool MeetsJobCondition() {
        if (JobCondition == null) {
            return false;
        }
        if (JobCondition.StartedQuestId > 0 &&
            (!session.Quest.TryGetQuest(JobCondition.StartedQuestId, out Quest? startedQuest) || startedQuest.State != QuestState.Started)) {
            return false;
        }

        if (JobCondition.CompletedQuestId > 0 &&
            (!session.Quest.TryGetQuest(JobCondition.CompletedQuestId, out Quest? completedQuest) || completedQuest.State != QuestState.Completed)) {
            return false;
        }

        if (JobCondition.JobCode != JobCode.None && session.Player.Value.Character.Job.Code() != JobCondition.JobCode) {
            return false;
        }

        // TODO: Maid checks

        if (JobCondition.BuffId > 0 && !session.Player.Buffs.Buffs.ContainsKey(JobCondition.BuffId)) {
            return false;
        }

        if (JobCondition.Mesos > 0 && session.Currency.Meso < JobCondition.Mesos) {
            return false;
        }

        if (JobCondition.Level > 0 && session.Player.Value.Character.Level < JobCondition.Level) {
            return false;
        }

        // TODO: Check if player is in home

        if (JobCondition.Guild && session.Player.Value.Character.GuildId == 0) {
            return false;
        }

        if (JobCondition.CompletedAchievement > 0 && !session.Achievement.HasAchievement(JobCondition.CompletedAchievement)) {
            return false;
        }

        // TODO: Check if it's the player's birthday

        if (JobCondition.MapId > 0 && session.Field?.MapId != JobCondition.MapId) {
            return false;
        }

        return true;
    }

    private void PerformJobScript() {
        if (State?.Type != ScriptStateType.Job) {
            return;
        }

        if (JobCondition == null) {
            return;
        }

        if (JobCondition.Mesos > 0) {
            if (session.Currency.CanAddMeso(-JobCondition.Mesos) != -JobCondition.Mesos) {
                session.Send(ChatPacket.Alert(StringCode.s_err_lack_meso));
                return;
            }
            session.Currency.Meso -= JobCondition.Mesos;
        }

        if (JobCondition.MoveMapId > 0) {
            session.Send(session.PrepareField(JobCondition.MoveMapId, portalId: JobCondition.MovePortalId > 0 ? JobCondition.MovePortalId : -1)
                ? FieldEnterPacket.Request(session.Player)
                : FieldEnterPacket.Error(MigrationError.s_move_err_default));
        }
    }

    private bool MeetsScriptCondition(ScriptConditionMetadata scriptCondition) {
        if (scriptCondition.JobCode.Count > 0 && !scriptCondition.JobCode.Contains(session.Player.Value.Character.Job.Code())) {
            return false;
        }

        foreach ((int questId, bool started) in scriptCondition.QuestStarted) {
            session.Quest.TryGetQuest(questId, out Quest? quest);
            if (started && (quest == null || quest.State != QuestState.Started)) {
                return false;
            }

            if (!started && quest != null && quest.State == QuestState.Started) {
                return false;
            }
        }

        foreach ((int questId, bool completed) in scriptCondition.QuestCompleted) {
            session.Quest.TryGetQuest(questId, out Quest? quest);
            if (completed && (quest == null || quest.State != QuestState.Completed)) {
                return false;
            }

            if (!completed && quest != null && quest.State == QuestState.Completed) {
                return false;
            }
        }

        foreach ((ItemComponent itemComponent, bool has) in scriptCondition.Items) {
            IEnumerable<Item> items = session.Item.Inventory.Find(itemComponent.ItemId, itemComponent.Rarity);
            int itemSum = items.Sum(item => item.Amount);
            if (has && itemSum < itemComponent.Amount) {
                return false;
            }

            if (!has && itemSum >= itemComponent.Amount) {
                return false;
            }
        }

        if (scriptCondition.Buff.Key > 0) {
            if (scriptCondition.Buff.Value && !session.Player.Buffs.Buffs.ContainsKey(scriptCondition.Buff.Key)) {
                return false;
            }
        }

        if (scriptCondition.Meso.Key > 0) {
            if (scriptCondition.Meso.Value && session.Currency.Meso < scriptCondition.Meso.Key) {
                return false;
            }

            if (!scriptCondition.Meso.Value && session.Currency.Meso >= scriptCondition.Meso.Key) {
                return false;
            }
        }

        if (scriptCondition.Level.Key > 0) {
            if (scriptCondition.Level.Value && session.Player.Value.Character.Level < scriptCondition.Level.Key) {
                return false;
            }

            if (!scriptCondition.Level.Value && session.Player.Value.Character.Level >= scriptCondition.Level.Key) {
                return false;
            }
        }

        if (scriptCondition.AchieveCompleted.Key > 0) {
            if (scriptCondition.AchieveCompleted.Value && !session.Achievement.HasAchievement(scriptCondition.AchieveCompleted.Key)) {
                return false;
            }

            if (!scriptCondition.AchieveCompleted.Value && session.Achievement.HasAchievement(scriptCondition.AchieveCompleted.Key)) {
                return false;
            }
        }

        if (scriptCondition.InGuild && session.Player.Value.Character.GuildId == 0) {
            return false;
        }

        return true;
    }

    private ScriptState? GetSelectScriptState() {
        if (metadata == null) {
            return null;
        }
        IList<ScriptState> scriptStates = new List<ScriptState>();
        foreach (ScriptState scriptState in metadata.States.Values.Where(state => state.Type == ScriptStateType.Select)) {
            if (scriptConditions == null) {
                scriptStates.Add(scriptState);
                continue;
            }

            if (!scriptConditions.TryGetValue(scriptState.Id, out ScriptConditionMetadata? scriptCondition)) {
                scriptStates.Add(scriptState);
                continue;
            }

            if (MeetsScriptCondition(scriptCondition)) {
                scriptStates.Add(scriptState);
            }
        }
        return scriptStates.Count == 0 ? null : scriptStates[Random.Shared.Next(scriptStates.Count)];
    }

    private ScriptState? GetFirstQuestScriptState() {
        if (Quests.Count == 0) {
            return null;
        }

        if (!session.ScriptMetadata.TryGet(Quests.Keys.Min(), out ScriptMetadata? questMetadata)) {
            return null;
        }

        QuestId = questMetadata.Id;
        return GetQuestScriptState(questMetadata);
    }

    private ScriptState? GetQuestScriptState(ScriptMetadata? scriptMetadata) {
        if (scriptMetadata == null) {
            return null;
        }
        session.Quest.TryGetQuest(scriptMetadata.Id, out Quest? quest);
        QuestState questState = quest?.State ?? QuestState.None;

        int stateId = questState switch {
            QuestState.None => GetFirstStateScript(scriptMetadata.States.Keys.ToArray(), 100, 200),
            QuestState.Started => GetFirstStateScript(scriptMetadata.States.Keys.ToArray(), 200, 300),
            _ => 0,
        };

        if (quest != null && session.Quest.CanStart(quest.Metadata.Require)) {
            stateId = GetFirstStateScript(scriptMetadata.States.Keys.ToArray(), 200, 300);
        }

        if (quest != null && quest.Metadata.Basic.CompleteNpc == Npc.Value.Id) {
            stateId = GetFirstStateScript(scriptMetadata.States.Keys.ToArray(), 300, 400);
        }

        return scriptMetadata.States.TryGetValue(stateId, out ScriptState? scriptState) ? scriptState : null;

        int GetFirstStateScript(IEnumerable<int> questStates, int lowerBound, int upperBound) {
            IEnumerable<int> statesInRange = questStates.Where(id => id >= lowerBound && id < upperBound);
            return statesInRange.Min();
        }
    }

    private void NpcRespond() {
        if (State == null) {
            session.Send(NpcTalkPacket.Close());
            return;
        }
        var dialogue = new NpcDialogue(State.Id, 0, GetButton());

        if (TalkType.HasFlag(NpcTalkType.Quest)) {
            session.Send(QuestPacket.Talk(Npc, Quests!.Values));
        }

        session.Send(NpcTalkPacket.Respond(Npc, TalkType, dialogue));
        ProcessScriptFunction();
    }

    private NpcTalkButton GetButton() {
        if (State == null) {
            return NpcTalkButton.None;
        }

        CinematicContent? content = State.Contents.ElementAtOrDefault(Index);
        if (content == null) {
            return NpcTalkButton.None;
        }
        if (content.ButtonType != NpcTalkButton.None) {
            return content.ButtonType;
        }

        if (content.Distractors.Length > 0) {
            return NpcTalkButton.SelectableDistractor;
        }

        if (Index < State.Contents.Length - 1) {
            return NpcTalkButton.Next;
        }

        if (TalkType.HasFlag(NpcTalkType.Select)) {
            if (State.Contents.Length > 0) {
                return NpcTalkButton.SelectableTalk;
            }
            return NpcTalkButton.None;
        }

        switch (State.Type) {
            case ScriptStateType.Job:
                switch (Npc.Value.Metadata.Basic.Kind) {
                    case >= 30 and < 40: // Beauty
                        return NpcTalkButton.SelectableBeauty;
                    case 80:
                        return NpcTalkButton.ChangeJob;
                    case 81:
                        return NpcTalkButton.PenaltyResolve;
                    case 82:
                        return NpcTalkButton.TakeBoat;
                    case 501:
                        return NpcTalkButton.Roulette;
                }
                break;
            case ScriptStateType.Select:
                switch (Npc.Value.Metadata.Basic.Kind) {
                    case 1 or > 10 and < 20: // Shop
                        return NpcTalkButton.None;
                    case 2: // Storage
                        return NpcTalkButton.None;
                }
                return NpcTalkButton.SelectableTalk;
            case ScriptStateType.Quest:
                switch (State.Id / 100) {
                    case 1:
                        return NpcTalkButton.QuestAccept;
                    case 2:
                        return NpcTalkButton.QuestProgress;
                    case 3:
                        return NpcTalkButton.QuestComplete;
                }
                break;
        }

        return NpcTalkButton.Close;
    }

    public void ProcessScriptFunction(bool enter = true) {
        if (State == null) {
            return;
        }

        if (!session.ServerTableMetadata.ScriptFunctionTable.Entries.TryGetValue(Npc.Value.Id, out Dictionary<int, Dictionary<int, ScriptFunctionMetadata>>? scriptFunctions) ||
            !scriptFunctions.TryGetValue(State.Id, out Dictionary<int, ScriptFunctionMetadata>? functions) ||
            !functions.TryGetValue(State.Contents.ElementAt(Index).FunctionId, out ScriptFunctionMetadata? scriptFunction)) {
            return;
        }

        if ((scriptFunction.EndFunction & enter) ||
            (!scriptFunction.EndFunction & !enter)) {
            return;
        }

        if (scriptFunction.PortalId > 0) {
            session.Send(NpcTalkPacket.MovePlayer(scriptFunction.PortalId));
        }

        if (!string.IsNullOrEmpty(scriptFunction.UiName)) {
            session.Send(NpcTalkPacket.OpenDialog(scriptFunction.UiName, scriptFunction.UiArg));
        }

        if (!string.IsNullOrEmpty(scriptFunction.MoveMapMovie)) {
            session.Send(NpcTalkPacket.Cutscene(scriptFunction.MoveMapMovie));
        }

        if (scriptFunction.CollectItems.Count > 0) {
            if (!session.Item.Inventory.ConsumeItemComponents(scriptFunction.CollectItems.ToList())) {
                logger.Error("Failed to consume items for script function {FunctionId} on npc {NpcId}", scriptFunction.FunctionId, Npc.Value.Id);
            }
        }

        if (scriptFunction.CollectMeso > 0) {
            session.Currency.Meso -= scriptFunction.CollectMeso;
        }

        IList<Item> items = new List<Item>();
        foreach (ItemComponent item in scriptFunction.PresentItems) {
            Item? newItem = session.Field.ItemDrop.CreateItem(item.ItemId, item.Rarity, item.Amount);
            if (newItem == null) {
                continue;
            }
            session.Item.Inventory.Add(newItem, true);
            items.Add(newItem);
        }

        if (items.Count > 0) {
            session.Send(NpcTalkPacket.RewardItem(items));
        }

        if (scriptFunction.PresentExp > 0) {
            session.Player.Value.Character.Exp += scriptFunction.PresentExp;
            session.Send(NpcTalkPacket.RewardExp(scriptFunction.PresentExp));
        }

        if (scriptFunction.MoveMapId > 0) {
            session.Send(session.PrepareField(scriptFunction.MoveMapId, portalId: scriptFunction.MovePortalId > 0 ? scriptFunction.MovePortalId : -1)
                ? FieldEnterPacket.Request(session.Player)
                : FieldEnterPacket.Error(MigrationError.s_move_err_default));
        }
    }
}
