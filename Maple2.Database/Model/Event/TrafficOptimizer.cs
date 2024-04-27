using System.Diagnostics.CodeAnalysis;

namespace Maple2.Database.Model.Event;

internal class TrafficOptimizer : GameEventInfo {
    public int GuideObjectSyncInterval { get; set; }
    public int RideSyncInterval { get; set; }
    public int LinearMovementInterval { get; set; }
    public int UserSyncInterval { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator TrafficOptimizer?(Maple2.Model.Game.Event.TrafficOptimizer? other) {
        return other == null ? null : new TrafficOptimizer {
            Id = other.Id,
            Name = other.Name,
            GuideObjectSyncInterval = other.GuideObjectSyncInterval,
            RideSyncInterval = other.RideSyncInterval,
            LinearMovementInterval = other.LinearMovementInterval,
            UserSyncInterval = other.UserSyncInterval,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.Event.TrafficOptimizer?(TrafficOptimizer? other) {
        return other == null ? null : new Maple2.Model.Game.Event.TrafficOptimizer {
            Id = other.Id,
            Name = other.Name,
            GuideObjectSyncInterval = other.GuideObjectSyncInterval,
            RideSyncInterval = other.RideSyncInterval,
            LinearMovementInterval = other.LinearMovementInterval,
            UserSyncInterval = other.UserSyncInterval,
        };
    }
}
