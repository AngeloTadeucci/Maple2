namespace Maple2.Model.Metadata;

public record PetMetadata(
    int Id,
    string? Name,
    int Type,
    string[] AiPresets,
    int NpcId,
    int ItemSlots,
    bool EnableExtraction,
    short OptionLevel,
    float OptionFactor,
    PetMetadataSkill? Skill,
    PetMetadataEffect[] Effect,
    PetMetadataDistance Distance,
    PetMetadataTime Time) : ISearchResult;

public record PetMetadataSkill(int Id, short Level);

public record PetMetadataEffect(int Id, short Level);

public record PetMetadataDistance(float Warp, float Trace, float BattleTrace);

public record PetMetadataTime(int Idle, int Bore, int Summon, int Tired, int Skill);
