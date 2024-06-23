using System;
using System.Collections.Generic;

namespace Maple2.Model.Metadata;

public record TimeEventTable(IReadOnlyDictionary<int, GlobalPortalMetadata> GlobalPortal) : ServerTable;

public record GlobalPortalMetadata(
    int Id,
    int Probability,
    DateTime StartTime,
    DateTime EndTime,
    TimeSpan CycleTime,
    TimeSpan RandomTime,
    TimeSpan LifeTime,
    string PopupMessage,
    string SoundId,
    GlobalPortalMetadata.Field[] Entries) {
    public record Field(
        string Name,
        int MapId,
        int PortalId);
}
