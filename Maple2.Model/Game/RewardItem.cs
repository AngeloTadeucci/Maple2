using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Maple2.Model.Game;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 14)]
public readonly record struct RewardItem {
    public int ItemId { get; }
    public short Rarity { get; }
    public int Amount { get; }
    public bool Unknown1 { get; }
    public bool Unknown2 { get; }
    public bool Unknown3 { get; }
    public bool Unknown4 { get; }

    [JsonConstructor]
    public RewardItem(int itemId, short rarity, int amount) {
        ItemId = itemId;
        Rarity = rarity;
        Amount = amount;
    }

    public static implicit operator RewardItem(Item item) {
        return new RewardItem(item.Id, (short) item.Rarity, item.Amount);
    }
}

public readonly struct RewardRecord {
    public ICollection<RewardItem>? Items { get; } = [];
    public long Exp { get; }
    public long PrestigeExp { get; }
    public long Meso { get; }

    public RewardRecord(List<RewardItem> items, long exp, long prestigeExp, long meso) {
        Items = items;
        Exp = exp;
        PrestigeExp = prestigeExp;
        Meso = meso;
    }

    public RewardRecord(List<Item> items, long exp, long prestigeExp, long meso) {
        Items = items.Select(item => (RewardItem) item).ToList();
        Exp = exp;
        PrestigeExp = prestigeExp;
        Meso = meso;
    }
}
