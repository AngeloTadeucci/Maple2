using System.Numerics;
using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record GlobalDropItemBoxTable(
    IDictionary<int, Dictionary<int, IList<GlobalDropItemBoxTable.Group>>> DropGroups,
    IDictionary<int, IList<GlobalDropItemBoxTable.Item>> Items) : ServerTable {

    public record Group(
        int GroupId,
        int MinLevel,
        int MaxLevel,
        IList<Group.DropCount> DropCounts,
        bool OwnerDrop,
        MapType MapTypeCondition,
        Continent ContinentCondition) {

        public record DropCount(
            int Amount,
            int Probability);
    }

    public record Item(
        int Id,
        int MinLevel,
        int MaxLevel,
        Range<int> DropCount,
        short Rarity,
        int Weight,
        int[] MapIds,
        bool QuestConstraint);
    public readonly record struct Range<T>(T Min, T Max) where T : INumber<T>;
}
