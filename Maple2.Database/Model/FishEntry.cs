using System.Diagnostics.CodeAnalysis;

namespace Maple2.Database.Model;

internal class FishEntry {
    public int Id { get; set; }
    public int TotalCaught { get; set; }
    public int TotalPrizeFish { get; set; }
    public int LargestSize { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator FishEntry?(Maple2.Model.Game.FishEntry? other) {
        return other == null ? null : new FishEntry {
            Id = other.Id,
            TotalCaught = other.TotalCaught,
            TotalPrizeFish = other.TotalPrizeFish,
            LargestSize = other.LargestSize,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.FishEntry?(FishEntry? other) {
        return other == null ? null : new Maple2.Model.Game.FishEntry(other.Id) {
            TotalCaught = other.TotalCaught,
            TotalPrizeFish = other.TotalPrizeFish,
            LargestSize = other.LargestSize,
        };
    }
}
