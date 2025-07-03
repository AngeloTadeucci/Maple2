using Maple2.Model.Game;

namespace Maple2.Model.Metadata;

public record ChapterBookTable(IReadOnlyDictionary<int, ChapterBookTable.Entry> Entries) : Table {
    public record Entry(
        int Id,
        int BeginQuestId,
        int EndQuestId,
        Entry.SkillPoint[] SkillPoints,
        int StatPoints,
        ItemComponent[] Items) {
        public record SkillPoint(int Amount, short Rank);
    }
}
