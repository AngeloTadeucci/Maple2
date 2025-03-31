using System.Collections.Concurrent;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;

namespace Maple2.Model.Game.Dungeon;

public class DungeonRoomRecord {
    public ConcurrentDictionary<long, DungeonUserRecord> UserResults = [];
    public readonly DungeonRoomMetadata Metadata;
    public long StartTick { get; set; }
    public long EndTick { get; set; }
    public DungeonState State { get; set; } = DungeonState.None;

    public DungeonRoomRecord(DungeonRoomMetadata metadata) {
        Metadata = metadata;
    }
}
