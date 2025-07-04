using Maple2.Model.Enum;

namespace Maple2.Model.Game;

public record ItemComponent(
    int ItemId,
    int Rarity,
    int Amount,
    ItemTag Tag);
