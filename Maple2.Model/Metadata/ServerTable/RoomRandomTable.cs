using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record RoomRandomTable(IReadOnlyDictionary<int, InstanceFieldMetadata> RandomEntries,
                              IReadOnlyDictionary<int, InstanceFieldMetadata> RoomEntry) : ServerTable;

public record RandomEntry(
    int MapId,
    InstanceType Type,
    int InstanceId,
    bool BackupSourcePortal,
    int PoolCount,
    bool SaveField,
    int NpcStatFactorId,
    int MaxCount,
    byte OpenType,
    int OpenValue
);

public record RoomEntry(
    int Id,
    int[] MapIds,
    int DurationTick,
    string AssetName,
    int MaxUserCount,
    bool AutoClose);
