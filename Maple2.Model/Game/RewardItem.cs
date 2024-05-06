﻿using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Maple2.Model.Game;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 14)]
[method: JsonConstructor]
public readonly struct RewardItem(int itemId, short rarity, int amount) {
    public int ItemId { get; } = itemId;
    public short Rarity { get; } = rarity;
    public int Amount { get; } = amount;
    public bool Unknown1 { get; }
    public bool Unknown2 { get; }
    public bool Unknown3 { get; }
    public bool Unknown4 { get; }

}
