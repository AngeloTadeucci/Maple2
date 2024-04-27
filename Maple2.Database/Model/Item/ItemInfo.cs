using System.Diagnostics.CodeAnalysis;
using Maple2.Model.Enum;

namespace Maple2.Database.Model;

internal record ItemTransfer(TransferFlag Flag, int RemainTrades, int RepackageCount, ItemBinding? Binding) {
    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator ItemTransfer?(Maple2.Model.Game.ItemTransfer? other) {
        return other == null ? null :
            new ItemTransfer(other.Flag, other.RemainTrades, other.RepackageCount, other.Binding);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.ItemTransfer?(ItemTransfer? other) {
        return other == null ? null :
            new Maple2.Model.Game.ItemTransfer(other.Flag, other.RemainTrades, other.RepackageCount, other.Binding);
    }
}

internal record ItemCoupleInfo(long CharacterId, string Name, bool IsCreator) {
    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator ItemCoupleInfo?(Maple2.Model.Game.ItemCoupleInfo? other) {
        return other == null ? null : new ItemCoupleInfo(other.CharacterId, other.Name, other.IsCreator);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.ItemCoupleInfo?(ItemCoupleInfo? other) {
        return other == null ? null : new Maple2.Model.Game.ItemCoupleInfo(other.CharacterId, other.Name, other.IsCreator);
    }
}

internal record ItemBinding(long CharacterId, string Name) {
    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator ItemBinding?(Maple2.Model.Game.ItemBinding? other) {
        return other == null ? null : new ItemBinding(other.CharacterId, other.Name);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.ItemBinding?(ItemBinding? other) {
        return other == null ? null : new Maple2.Model.Game.ItemBinding(other.CharacterId, other.Name);
    }
}
