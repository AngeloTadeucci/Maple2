using System.Diagnostics.CodeAnalysis;

namespace Maple2.Database.Model.Event;

internal class StringBoardLink : GameEventInfo {
    public string Url { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator StringBoardLink?(Maple2.Model.Game.Event.StringBoardLink? other) {
        return other == null ? null : new StringBoardLink {
            Id = other.Id,
            Name = other.Name,
            Url = other.Url,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.Event.StringBoardLink?(StringBoardLink? other) {
        return other == null ? null : new Maple2.Model.Game.Event.StringBoardLink {
            Id = other.Id,
            Name = other.Name,
            Url = other.Url,
        };
    }
}
