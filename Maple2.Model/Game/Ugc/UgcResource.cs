using Maple2.Model.Enum;

namespace Maple2.Model.Game;

public class UgcResource {
    public long Id { get; init; }
    public string Path { get; set; }
    public UgcType Type { get; init; }

    public UgcResource() {
        Path = string.Empty;
    }

}
