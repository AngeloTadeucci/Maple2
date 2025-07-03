namespace Maple2.Model.Metadata;

public record GachaInfoTable(IReadOnlyDictionary<int, GachaInfoTable.Entry> Entries) : Table {
    public record Entry(
        byte RandomBoxGroup,
        int DropBoxId,
        int ShopId,
        int CoinItemId,
        int CoinItemAmount);
}
