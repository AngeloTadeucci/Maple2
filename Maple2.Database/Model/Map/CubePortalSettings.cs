using System.Diagnostics.CodeAnalysis;
using Maple2.Model.Enum;
using Maple2.Model.Game;

namespace Maple2.Database.Model;

internal class CubePortalSettings {
    public string PortalName { get; set; }
    public PortalActionType Method { get; set; }
    public CubePortalDestination Destination { get; set; }
    public string DestinationTarget { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator CubePortalSettings?(Maple2.Model.Game.CubePortalSettings? other) {
        return other == null ? null : new CubePortalSettings {
            PortalName = other.PortalName,
            Method = other.Method,
            Destination = other.Destination,
            DestinationTarget = other.DestinationTarget,
        };
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
