namespace Maple2.Model.Enum;

public enum QuestType {
    EpicQuest = 0,
    WorldQuest = 1,
    EventQuest = 2,
    DailyMission = 3, // Navigator
    FieldMission = 4, // Exploration
    EventMission = 5,
    GuildQuest = 6,
    MentoringMission = 7,
    FieldQuest = 8,
    AllianceQuest = 9,
    WeddingMission = 10,
}

public enum QuestState {
    None = 0,
    Started = 1,
    Completed = 2,
}

public enum QuestRemoteType {
    None = 0,
    Cinematic = 1,
    Popup = 2,
    System = 3,
}

public enum QuestDispatchType {
    None,
    MonologueAccept,
    MonologueComplete,
    DirectAccept,
    DirectComplete,
}

public enum QuestEventMissionType {
    none,
    gallery,
    levelup_package,
    stamp,
    timerun,
}
