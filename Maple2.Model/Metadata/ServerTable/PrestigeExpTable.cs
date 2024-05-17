using System.Collections.Generic;
using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record PrestigeExpTable(
    IReadOnlyDictionary<ExpType, long> Entries) : ServerTable;

