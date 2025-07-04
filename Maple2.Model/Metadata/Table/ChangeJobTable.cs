using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record ChangeJobTable(IReadOnlyDictionary<Job, ChangeJobMetadata> Entries) : Table;

public record ChangeJobMetadata(
    Job Job,
    Job ChangeJob,
    int StartQuestId,
    int EndQuestId);
