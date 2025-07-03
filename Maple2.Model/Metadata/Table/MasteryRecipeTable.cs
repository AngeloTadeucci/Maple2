using Maple2.Model.Enum;
using Maple2.Model.Game;

namespace Maple2.Model.Metadata;

public record MasteryRecipeTable(IReadOnlyDictionary<int, MasteryRecipeTable.Entry> Entries) : Table {
    public record Entry(
        int Id,
        MasteryType Type,
        bool NoRewardExp,
        int RequiredMastery,
        long RequiredMeso,
        IReadOnlyList<int> RequiredQuests,
        long RewardExp,
        int RewardMastery,
        int HighRateLimitCount,
        int NormalRateLimitCount,
        IReadOnlyList<ItemComponent> RequiredItems,
        IReadOnlyList<int> HabitatMapId,
        IReadOnlyList<ItemComponent> RewardItems);
}


