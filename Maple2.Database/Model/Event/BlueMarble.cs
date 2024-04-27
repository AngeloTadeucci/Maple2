using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Maple2.Model.Game.Event;

namespace Maple2.Database.Model.Event;

internal class BlueMarble : GameEventInfo {
    public IList<BlueMarbleEntry> Entries { get; set; }
    public IList<BlueMarbleTile> Tiles { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator BlueMarble?(Maple2.Model.Game.Event.BlueMarble? other) {
        return other == null ? null : new BlueMarble {
            Id = other.Id,
            Name = other.Name,
            Entries = other.Entries,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.Event.BlueMarble?(BlueMarble? other) {
        return other == null ? null : new Maple2.Model.Game.Event.BlueMarble {
            Id = other.Id,
            Name = other.Name,
            Entries = other.Entries,
        };
    }
}
