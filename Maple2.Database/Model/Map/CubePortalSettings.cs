using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Maple2.Model.Enum;
using Maple2.Model.Game;

namespace Maple2.Database.Model;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "!")]
[JsonDerivedType(typeof(CubePortalSettings), typeDiscriminator: "cube-portal")]
internal abstract record CubeSettings;

internal record CubePortalSettings(
    string PortalName,
    PortalActionType Method,
    CubePortalDestination Destination,
    string DestinationTarget) : CubeSettings {
    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator CubePortalSettings?(Maple2.Model.Game.CubePortalSettings? other) {
        return other == null ? null : new CubePortalSettings(
            other.PortalName,
            other.Method,
            other.Destination,
            other.DestinationTarget);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.CubePortalSettings?(CubePortalSettings? other) {
        return other == null ? null : new Maple2.Model.Game.CubePortalSettings {
            PortalName = other.PortalName,
            Method = other.Method,
            Destination = other.Destination,
            DestinationTarget = other.DestinationTarget,
        };
    }
}
