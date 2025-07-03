using System.Numerics;
using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record FishTable(
    IReadOnlyDictionary<int, FishTable.Fish> Fishes,
    IReadOnlyDictionary<int, FishTable.Spot> Spots,
    IReadOnlyDictionary<int, FishTable.Lure> Lures,
    IReadOnlyDictionary<int, FishTable.FishBox> GlobalFishBoxes,
    IReadOnlyDictionary<int, FishTable.FishBox> IndividualFishBoxes) : ServerTable {

    public record Fish(
        int Id,
        LiquidType FluidHabitat,
        int Mastery,
        int Level,
        short Rarity,
        int PointCount,
        int MasteryExp,
        int Exp,
        int FishingTime,
        int CatchProbability,
        int BaitProbability,
        Range<int> SmallSize,
        Range<int> BigSize,
        int[] BaitEffectIds,
        int IndividualDropBoxId,
        bool IgnoreSpotMastery);

    public readonly record struct Range<T>(T Min, T Max) where T : INumber<T>;

    public record Spot(
        int Id,
        int MinMastery,
        int MaxMastery,
        IReadOnlyList<LiquidType> LiquidTypes,
        int GlobalFishBoxId,
        int IndividualFishBoxId,
        int GlobalDropBoxId,
        int IndividualDropBoxId,
        int SpotLevel,
        int DropRank);

    public record Lure(
        int BuffId,
        short BuffLevel,
        Lure.Catch[] Catches,
        Lure.Spawn[] Spawns,
        int GlobalDropBoxId,
        int GlobalDropRank,
        int IndividualDropBoxId,
        int IndividualDropRank) {
        public record Catch(
            int Rank,
            int Probability);

        public record Spawn(
            int FishId,
            int Rate);
    }

    public record FishBox(
        int Id,
        int Probability,
        int CubeRate,
        IReadOnlyDictionary<int, int> Fishes) { // FishId, Weight
    }


}
