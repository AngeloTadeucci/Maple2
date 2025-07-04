using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record JobTable(IReadOnlyDictionary<JobCode, JobTable.Entry> Entries) : Table {
    public record Entry(
        Tutorial Tutorial,
        IReadOnlyDictionary<SkillRank, Skill[]> Skills,
        int[] BaseSkills);

    public record Tutorial(
        int StartField,
        int SkipField,
        int SkipItem,
        int[] OpenMaps,
        int[] OpenTaxis,
        Item[] StartItem,
        Item[] Reward);

    public record Skill(
        int Main,
        int[] Sub,
        short MaxLevel,
        int QuickSlot);

    public record Item(
        int Id,
        int Rarity,
        int Count);
}
