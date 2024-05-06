using Maple2.Model.Enum;

namespace Maple2.Model.Game;

public class UgcResource {
    public long Id { get; init; }
    public string Path { get; set; } = string.Empty;
    public UgcType Type { get; init; }

}
