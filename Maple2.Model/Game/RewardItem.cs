using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Maple2.Model.Game;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 14)]
public readonly struct RewardItem {
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
}
