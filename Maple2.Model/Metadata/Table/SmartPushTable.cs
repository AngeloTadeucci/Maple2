using Maple2.Model.Enum;
using Maple2.Model.Game;

namespace Maple2.Model.Metadata;

public record SmartPushTable(IReadOnlyDictionary<int, SmartPushMetadata> Entries) : Table;

public record SmartPushMetadata(
    int Id,
    string Content,
    SmartPushType Type,
    long Value,
    long MeretCost,
    IngredientInfo RequiredItem);
