using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record UgcDesignTable(IReadOnlyDictionary<int, UgcDesignTable.Entry> Entries) : Table {
    public record Entry(
        short ItemRarity,
        MeretMarketCurrencyType CurrencyType,
        long CreatePrice,
        long MarketMinPrice,
        long MarketMaxPrice);
}

