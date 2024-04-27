using System.Diagnostics.CodeAnalysis;

namespace Maple2.Database.Model.Event;

internal class MeratMarketNotice : GameEventInfo {
    public string Message { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator MeratMarketNotice?(Maple2.Model.Game.Event.MeratMarketNotice? other) {
        return other == null ? null : new MeratMarketNotice {
            Id = other.Id,
            Name = other.Name,
            Message = other.Message,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.Event.MeratMarketNotice?(MeratMarketNotice? other) {
        return other == null ? null : new Maple2.Model.Game.Event.MeratMarketNotice {
            Id = other.Id,
            Name = other.Name,
            Message = other.Message,
        };
    }
}
