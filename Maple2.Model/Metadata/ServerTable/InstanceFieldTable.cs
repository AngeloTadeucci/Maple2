using System.Collections.Generic;
using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record InstanceFieldTable(IReadOnlyDictionary<int, InstanceFieldMetadata> Entries) : ServerTable;

public record InstanceFieldMetadata(
    InstanceType type,
    int instanceId,
    bool backupSourcePortal,
    int poolCount,
    bool isSaveField,
    int npcStatFactorID,
    int maxCount,
    byte openType,
    int openValue
);