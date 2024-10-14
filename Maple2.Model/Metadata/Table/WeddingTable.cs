using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record WeddingTable(IReadOnlyDictionary<MarriageExpType, WeddingReward> Rewards,
                                 IReadOnlyDictionary<int, WeddingPackage> Packages) : Table;

public record WeddingReward(
    MarriageExpType Type,
    int Amount,
    MarriageExpLimit Limit);

public record WeddingPackage(
    int Id,
    int PlannerId,
    IReadOnlyDictionary<int, WeddingPackage.HallData> Halls) {
    public record HallData(
        int Id,
        int MapId,
        int NightMapId,
        int Tier,
        int MeretCost,
        IReadOnlyList<HallData.Item> Items,
        IReadOnlyList<HallData.Item> CompleteItems) {

        public record Item(
            int ItemId,
            short Rarity,
            int Amount,
            bool NightOnly);
    }
}
