using System;
using System.Diagnostics.CodeAnalysis;

namespace Maple2.Database.Model;

internal record ItemSocket(byte MaxSlots, ItemGemstone[] Sockets) {
    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator ItemSocket?(Maple2.Model.Game.ItemSocket? other) {
        if (other == null) {
            return null;
        }

        ItemGemstone[] sockets = Array.ConvertAll(other.Sockets, gemstone => (ItemGemstone) gemstone!);
        return new ItemSocket(other.MaxSlots, sockets);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.ItemSocket?(ItemSocket? other) {
        if (other == null) {
            return null;
        }

        Maple2.Model.Game.ItemGemstone[] sockets =
            Array.ConvertAll(other.Sockets, gemstone => (Maple2.Model.Game.ItemGemstone) gemstone);
        return new Maple2.Model.Game.ItemSocket(other.MaxSlots, sockets);
    }
}

internal record ItemGemstone(int ItemId, ItemBinding Binding, bool IsLocked, long UnlockTime) {
    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator ItemGemstone?(Maple2.Model.Game.ItemGemstone? other) {
        return other == null ? null :
            new ItemGemstone(other.ItemId, other.Binding!, other.IsLocked, other.UnlockTime);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.ItemGemstone?(ItemGemstone? other) {
        return other == null ? null :
            new Maple2.Model.Game.ItemGemstone(other.ItemId, other.Binding, other.IsLocked, other.UnlockTime);
    }
}
