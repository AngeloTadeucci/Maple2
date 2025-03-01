using System.Collections.Concurrent;
using Maple2.Database.Storage;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Model.Room;

public class DungeonFieldManager : FieldManager {
    public DungeonRoomTable.DungeonRoomMetadata Metadata { get; private set; }
    public int DungeonId => Metadata.Id;

    public readonly ConcurrentDictionary<int, DungeonFieldManager> RoomFields = [];

    public DungeonFieldManager(DungeonRoomTable.DungeonRoomMetadata metadata, MapMetadata mapMetadata, UgcMapMetadata ugcMetadata, MapEntityMetadata entities, NpcMetadataStorage npcMetadata, long ownerId = 0)
        : base(mapMetadata, ugcMetadata, entities, npcMetadata, ownerId) {
        Metadata = metadata;
    }
}
