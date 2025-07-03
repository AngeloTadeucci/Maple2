using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record RideMetadata(
    int Id,
    string Model,
    RideMetadataBasic Basic,
    RideMetadataSpeed Speed,
    IReadOnlyDictionary<BasicAttribute, long> Stats);

public record RideMetadataBasic(
    RideOnType Type,
    int SkillSetId,
    float SummonTime,
    long RunXStamina,
    bool EnableSwim,
    int FallDamageDown,
    int Passengers);

public record RideMetadataSpeed(
    float WalkSpeed,
    float RunSpeed,
    float RunXSpeed,
    float SwimSpeed);
