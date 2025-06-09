using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class QuestPacket {
    private enum Command : byte {
        Error = 0,
        Talk = 1,
        Start = 2,
        Update = 3,
        Complete = 4,
        Unknown5 = 5,
        Abandon = 6,
        Expired = 7,
        SetTracking = 9,
        SummonPortal = 18,
        ExplorationProgress = 21,
        LoadQuestStates = 22,
        LoadQuests = 23,
        Unknown25 = 25,
        ExplorationReward = 26,
        Unknown30 = 30,
        DailyReputationMissions = 31,
        WeeklyReputationMissions = 32,
        AllianceAccept = 34,
        AllianceComplete = 35,
        Unknown38 = 38,
    }

    public static ByteWriter Error(QuestError error) {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.Error);
        pWriter.Write<QuestError>(error);

        return pWriter;
    }

    public static ByteWriter Talk(FieldNpc npc, ICollection<QuestMetadata> quests) {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.Talk);
        pWriter.WriteInt(npc.ObjectId);
        pWriter.WriteInt(quests.Count);
        foreach (QuestMetadata quest in quests) {
            pWriter.WriteInt(quest.Id);
        }

        return pWriter;
    }

    public static ByteWriter Start(Quest quest) {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.Start);
        pWriter.WriteInt(quest.Id);
        pWriter.WriteLong(quest.StartTime);
        pWriter.WriteBool(quest.Track);
        pWriter.WriteInt(quest.Conditions.Count);
        foreach (Quest.Condition condition in quest.Conditions.Values) {
            pWriter.WriteInt(condition.Counter);
        }

        return pWriter;
    }

    public static ByteWriter Update(Quest quest) {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteInt(quest.Id);
        pWriter.WriteInt(quest.Conditions.Count);
        foreach (Quest.Condition condition in quest.Conditions.Values) {
            pWriter.WriteInt(condition.Counter);
        }

        return pWriter;
    }

    public static ByteWriter Complete(Quest quest) {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.Complete);
        pWriter.WriteInt(quest.Id);
        pWriter.WriteInt(1); // quest.State??
        pWriter.WriteLong(quest.EndTime);

        return pWriter;
    }

    public static ByteWriter Unknown5() {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.Unknown5);
        pWriter.WriteInt();
        pWriter.WriteInt();

        return pWriter;
    }

    public static ByteWriter Abandon(int questId) {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.Abandon);
        pWriter.WriteInt(questId);

        return pWriter;
    }

    public static ByteWriter Expired(ICollection<int> questIds) {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.Expired);
        pWriter.WriteInt(questIds.Count);
        foreach (int questId in questIds) {
            pWriter.WriteInt(questId);
        }

        return pWriter;
    }

    public static ByteWriter SetTracking(int questId, bool tracked) {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.SetTracking);
        pWriter.WriteInt(questId);
        pWriter.WriteBool(tracked);

        return pWriter;
    }

    public static ByteWriter SummonPortal(int npcObjectId, int portalId, int startTick) {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.SummonPortal);
        pWriter.WriteInt(npcObjectId);
        pWriter.WriteInt(portalId);
        pWriter.WriteInt(startTick);

        return pWriter;
    }

    public static ByteWriter LoadExploration(int starAmount) {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.ExplorationProgress);
        pWriter.WriteInt(starAmount);
        pWriter.WriteInt();

        return pWriter;
    }

    public static ByteWriter LoadQuestStates(ICollection<Quest> quests) {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.LoadQuestStates);
        pWriter.WriteInt(quests.Count);
        foreach (Quest quest in quests) {
            pWriter.WriteClass<Quest>(quest);
        }

        return pWriter;
    }

    public static ByteWriter LoadQuests(ICollection<int> questIds) {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.LoadQuests);
        pWriter.WriteInt(questIds.Count);
        foreach (int questId in questIds) {
            pWriter.WriteInt(questId);
        }

        return pWriter;
    }

    // CMentoringMission?
    public static ByteWriter Unknown25() {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.Unknown25);
        pWriter.WriteLong();

        return pWriter;
    }

    public static ByteWriter UpdateExploration(int starAmount) {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.ExplorationReward);
        pWriter.WriteInt(starAmount);

        return pWriter;
    }

    // CFieldQuest?
    public static ByteWriter Unknown30() {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.Unknown30);

        return pWriter;
    }

    public static ByteWriter LoadSkyFortressMissions(ICollection<int> questIds) {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.DailyReputationMissions);
        pWriter.WriteBool(true);
        pWriter.WriteInt(questIds.Count);
        foreach (int questId in questIds) {
            pWriter.WriteInt(questId);
        }

        return pWriter;
    }

    public static ByteWriter LoadKritiasMissions(ICollection<int> questIds) {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.WeeklyReputationMissions);
        pWriter.WriteBool(true);
        pWriter.WriteInt(questIds.Count);
        foreach (int questId in questIds) {
            pWriter.WriteInt(questId);
        }

        return pWriter;
    }

    // s_quest_alliance_accept_all_*
    public static ByteWriter AllianceAccept(AllianceType type) {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.AllianceAccept);
        pWriter.Write<AllianceType>(type);

        return pWriter;
    }

    // s_quest_alliance_complete_all_*
    public static ByteWriter AllianceComplete(AllianceType type) {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.AllianceComplete);
        pWriter.Write<AllianceType>(type);

        return pWriter;
    }

    public static ByteWriter Unknown38() {
        var pWriter = Packet.Of(SendOp.Quest);
        pWriter.Write<Command>(Command.Unknown38);

        return pWriter;
    }
}
