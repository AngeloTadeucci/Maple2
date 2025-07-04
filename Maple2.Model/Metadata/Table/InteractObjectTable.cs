using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record InteractObjectTable(IReadOnlyDictionary<int, InteractObjectMetadata> Entries) : Table;

public record InteractObjectMetadata(
    int Id,
    InteractType Type,
    int Collection,
    int ReactCount,
    int TargetPortalId,
    int GuildPosterId,
    int WeaponItemId,
    InteractObjectMetadataItem Item,
    InteractObjectMetadataTime Time,
    InteractObjectMetadataDrop Drop,
    InteractObjectMetadataEffect AdditionalEffect,
    InteractObjectMetadataSpawn[] Spawn);

public record InteractObjectMetadataItem(int Id, int Amount, int Rarity, int CheckAmount, int RecipeId);

public record InteractObjectMetadataTime(int Reset, int React, int Hide);

public record InteractObjectMetadataDrop(int Rarity, int[] GlobalDropBoxIds, int[] IndividualDropBoxIds, float DropHeight, float DropDistance);

public record InteractObjectMetadataEffect(
    InteractObjectMetadataEffect.ConditionEffect[] Condition,
    InteractObjectMetadataEffect.InvokeEffect[] Invoke,
    int ModifyCode,
    int ModifyTime
) {
    public record ConditionEffect(int Id, short Level);
    public record InvokeEffect(int Id, short Level, int Probability);
}

public record InteractObjectMetadataSpawn(int Id, int Radius, int Count, int Probability, int LifeTime);
