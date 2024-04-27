using System.Diagnostics.CodeAnalysis;

namespace Maple2.Database.Model.Event;

internal class EventFieldPopup : GameEventInfo {
    public int MapId { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator EventFieldPopup?(Maple2.Model.Game.Event.EventFieldPopup? other) {
        return other == null ? null : new EventFieldPopup {
            Id = other.Id,
            Name = other.Name,
            MapId = other.MapId,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.Event.EventFieldPopup?(EventFieldPopup? other) {
        return other == null ? null : new Maple2.Model.Game.Event.EventFieldPopup {
            Id = other.Id,
            Name = other.Name,
            MapId = other.MapId,
        };
    }
}
