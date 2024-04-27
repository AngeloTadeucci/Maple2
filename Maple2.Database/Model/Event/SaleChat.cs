using System.Diagnostics.CodeAnalysis;

namespace Maple2.Database.Model.Event;

internal class SaleChat : GameEventInfo {
    public int WorldChatDiscount { get; set; }
    public int ChannelChatDiscount { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator SaleChat?(Maple2.Model.Game.Event.SaleChat? other) {
        return other == null ? null : new SaleChat {
            Id = other.Id,
            Name = other.Name,
            WorldChatDiscount = other.WorldChatDiscount,
            ChannelChatDiscount = other.ChannelChatDiscount,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.Event.SaleChat?(SaleChat? other) {
        return other == null ? null : new Maple2.Model.Game.Event.SaleChat {
            Id = other.Id,
            Name = other.Name,
            WorldChatDiscount = other.WorldChatDiscount,
            ChannelChatDiscount = other.ChannelChatDiscount,
        };
    }
}
