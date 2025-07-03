namespace Maple2.Model.Metadata;

public record PremiumClubTable(
    IReadOnlyDictionary<int, PremiumClubTable.Buff> Buffs,
    IReadOnlyDictionary<int, PremiumClubTable.Item> Items,
    IReadOnlyDictionary<int, PremiumClubTable.Package> Packages) : Table {

    public record Buff(
        int Id,
        short Level);

    public record Item(
        int Id,
        int Amount,
        int Rarity,
        int Period);

    public record Package(
        bool Disabled,
        long StartDate,
        long EndDate,
        long Period,
        long Price,
        IList<Item> BonusItems);
}
