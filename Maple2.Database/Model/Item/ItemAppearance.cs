using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.Json.Serialization;
using Maple2.Model.Common;

namespace Maple2.Database.Model;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "!")]
[JsonDerivedType(typeof(ColorAppearance), typeDiscriminator: "default")]
[JsonDerivedType(typeof(HairAppearance), typeDiscriminator: "hair")]
[JsonDerivedType(typeof(DecalAppearance), typeDiscriminator: "decal")]
[JsonDerivedType(typeof(CapAppearance), typeDiscriminator: "cap")]
internal abstract record ItemAppearance(EquipColor Color);

internal record ColorAppearance(EquipColor Color) : ItemAppearance(Color) {
    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator ColorAppearance?(Maple2.Model.Game.ItemAppearance? other) {
        return other == null ? null : new ColorAppearance(other.Color);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.ItemAppearance?(ColorAppearance? other) {
        return other == null ? null : new Maple2.Model.Game.ItemAppearance(other.Color);
    }
}

internal record HairAppearance(EquipColor Color, float BackLength, Vector3 BackPosition1, Vector3 BackPosition2,
                               float FrontLength, Vector3 FrontPosition1, Vector3 FrontPosition2) : ItemAppearance(Color) {
    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator HairAppearance?(Maple2.Model.Game.HairAppearance? other) {
        return other == null ? null : new HairAppearance(other.Color, other.BackLength, other.BackPosition1,
            other.BackPosition2, other.FrontLength, other.FrontPosition1, other.FrontPosition2);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.HairAppearance?(HairAppearance? other) {
        return other == null ? null : new Maple2.Model.Game.HairAppearance(other.Color, other.BackLength,
            other.BackPosition1, other.BackPosition2, other.FrontLength, other.FrontPosition1, other.FrontPosition2);
    }
}

internal record DecalAppearance(EquipColor Color, float Position1, float Position2, float Position3, float Position4) : ItemAppearance(Color) {
    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator DecalAppearance?(Maple2.Model.Game.DecalAppearance? other) {
        return other == null ? null : new DecalAppearance(other.Color, other.Position1, other.Position2,
            other.Position3, other.Position4);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.DecalAppearance?(DecalAppearance? other) {
        return other == null ? null : new Maple2.Model.Game.DecalAppearance(other.Color, other.Position1,
            other.Position2, other.Position3, other.Position4);
    }
}

internal record CapAppearance(EquipColor Color, Vector3 Position1, Vector3 Position2, Vector3 Position3,
        Vector3 Position4, float Unknown) : ItemAppearance(Color) {
    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator CapAppearance?(Maple2.Model.Game.CapAppearance? other) {
        return other == null ? null : new CapAppearance(other.Color, other.Position1, other.Position2,
            other.Position3, other.Position4, other.Unknown);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.CapAppearance?(CapAppearance? other) {
        return other == null ? null : new Maple2.Model.Game.CapAppearance(other.Color, other.Position1,
            other.Position2, other.Position3, other.Position4, other.Unknown);
    }
}
