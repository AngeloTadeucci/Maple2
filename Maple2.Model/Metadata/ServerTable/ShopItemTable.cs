using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Game.Shop;

namespace Maple2.Model.Metadata;

public record ShopItemTable(IReadOnlyDictionary<int, Dictionary<int, ShopItemMetadata>> Entries) : ServerTable;

public record ShopItemMetadata(
    int Id,
    int ShopId,
    int ItemId,
    byte Rarity,
    ShopCost Cost,
    int SellCount,
    string Category,
    ShopItemMetadata.Requirement Requirements,
    RestrictedBuyData? RestrictedBuyData,
    short SellUnit,
    ShopItemLabel Label,
    string IconTag,
    bool WearForPreview,
    bool RandomOption,
    long Probability,
    bool IsPremiumItem) {

    public record Requirement(
        int GuildTrophy,
        Achievement Achievement,
        Championship Championship,
        GuildNpc GuildNpc,
        QuestAlliance QuestAlliance);

    public record Achievement(
        int Id,
        int Rank
    );

    public record Championship(
        byte Rank,
        short JoinCount);

    public record GuildNpc(
        GuildNpcType Type,
        short Level);

    public record QuestAlliance(
        ReputationType Type,
        int Grade);
}
