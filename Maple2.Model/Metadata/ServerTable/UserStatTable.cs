using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record UserStatTable(
    IReadOnlyDictionary<JobCode, IReadOnlyDictionary<short, IReadOnlyDictionary<BasicAttribute, long>>> JobStats
) : ServerTable;
