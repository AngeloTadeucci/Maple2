using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Storage;
using Maple2.Model.Game.Party;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Model.Room;

public class DungeonFieldManager : FieldManager {
    public DungeonRoomTable.DungeonRoomMetadata Metadata { get; private set; }
    public int DungeonId => Metadata.Id;

    public DungeonFieldManager? Lobby { get; init; }
    public readonly ConcurrentDictionary<int, DungeonFieldManager> RoomFields = [];

    public Party? Party { get; init; }

    public DungeonFieldManager(DungeonRoomTable.DungeonRoomMetadata metadata, MapMetadata mapMetadata, UgcMapMetadata ugcMetadata, MapEntityMetadata entities, NpcMetadataStorage npcMetadata, long ownerId = 0, Party? party = null)
        : base(mapMetadata, ugcMetadata, entities, npcMetadata, ownerId) {
        Party = party;
        Metadata = metadata;
        if (metadata.LobbyFieldId == mapMetadata.Id) {
            Lobby = this;
        }
    }
}
