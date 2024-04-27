using System.Diagnostics.CodeAnalysis;

namespace Maple2.Database.Model.Event;

internal class StringBoard : GameEventInfo {
    public int StringId { get; set; }
    public string String { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator StringBoard?(Maple2.Model.Game.Event.StringBoard? other) {
        return other == null ? null : new StringBoard {
            Id = other.Id,
            Name = other.Name,
            StringId = other.StringId,
            String = other.String,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.Event.StringBoard?(StringBoard? other) {
        return other == null ? null : new Maple2.Model.Game.Event.StringBoard {
            Id = other.Id,
            Name = other.Name,
            StringId = other.StringId,
            String = other.String,
        };
    }
}
