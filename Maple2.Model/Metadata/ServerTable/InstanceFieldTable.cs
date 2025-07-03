using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record InstanceFieldTable(IReadOnlyDictionary<int, InstanceFieldMetadata> Entries) : ServerTable;

public record InstanceFieldMetadata(
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
