namespace Maple2.Model.Metadata;

public record LearningQuestTable(IReadOnlyDictionary<int, LearningQuestTable.Entry> Entries) : Table {
    public record Entry(
        int Category,
        int RequiredLevel,
        int QuestId,
        int RequiredMapId,
        int GoToMapId,
        int GoToPortalId
        );
}
