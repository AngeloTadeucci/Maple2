using System;
using System.Runtime.InteropServices;

namespace Maple2.Model.Game;

public class PetConfig(PetPotionConfig[]? potionConfig = null, PetLootConfig? lootConfig = null) {
    public PetPotionConfig[] PotionConfig = potionConfig ?? Array.Empty<PetPotionConfig>();
    public PetLootConfig LootConfig = lootConfig ?? new PetLootConfig(true, true, true, true, true, true, true, false, 1, true);

}

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
public readonly record struct PetPotionConfig(
    int Index,
    float Threshold,
    int ItemId);

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 13)]
public readonly record struct PetLootConfig(
    bool Mesos,
    bool Merets,
    bool Other,
    bool Currency,
    bool Equipment,
    bool Consumable,
    bool Gemstone,
    bool Dropped,
    int Rarity,
    bool Enabled);
