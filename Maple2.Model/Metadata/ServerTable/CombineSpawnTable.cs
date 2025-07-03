using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record CombineSpawnTable(IReadOnlyDictionary<int, Dictionary<int, SpawnGroupMetadata>> Groups, // Key 1: MapId, Key 2: groupId
                              IReadOnlyDictionary<int, Dictionary<int, SpawnNpcMetadata>> Npcs,
                              IReadOnlyDictionary<int, Dictionary<int, SpawnInteractObjectMetadata>> InteractObjects) : ServerTable; // Key 1: groupId, Key 2: combineId

public record SpawnGroupMetadata(
    int GroupId,
    CombineSpawnGroupType Type,
    int TotalCount,
    int ResetTick,
    int MapId);

public record SpawnNpcMetadata(
    int CombineId,
    int GroupId,
    int Weight,
    int SpawnId);

public record SpawnInteractObjectMetadata(
    int CombineId,
    int GroupId,
    int Weight,
    int RegionSpawnId,
    int InteractId,
    string Model,
    string Asset,
    string Normal,
    string Reactable,
    float Scale,
    bool KeepAnimate);
