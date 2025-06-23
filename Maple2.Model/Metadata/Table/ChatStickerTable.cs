namespace Maple2.Model.Metadata;

public record ChatStickerTable(IReadOnlyDictionary<int, ChatStickerMetadata> Entries) : Table;

public record ChatStickerMetadata(int Id, int GroupId);
