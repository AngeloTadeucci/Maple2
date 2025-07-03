using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using System.Numerics;
using Maple2.Model.Game;

namespace Maple2.Server.Game.Model.ActorStateComponent;

public partial class MovementState {
    private class DebugMarker {
        public bool Spawned;
        public Item ItemData;
        public FieldItem Item;
        public long NextUpdate = 0;
        public int ObjectId = 0;

        public DebugMarker(FieldItem item, Item itemData) {
            Item = item;
            ItemData = itemData;
        }
    }
    private DebugMarker debugNpc;
    private DebugMarker debugTarget;
    private DebugMarker debugAgent;

    private DebugMarker InitDebugMarker(int itemId, int rarity) {
        actor.Field.ItemMetadata.TryGet(itemId, out ItemMetadata? itemData);

        if (itemData is null) {
            throw new InvalidDataException("bad item");
        }

        Item? item = actor.Field.ItemDrop.CreateItem(itemId, rarity);
        if (item == null) {
            throw new InvalidDataException("bad item");
        }

        FieldItem fieldItem = new FieldItem(actor.Field, 0, item) {
            FixedPosition = true,
            ReceiverId = -1,
            Type = DropType.Player,
        };

        return new DebugMarker(fieldItem, item);
    }

    private void UpdateDebugMarker(Vector3 position, DebugMarker marker, long tickCount) {
        if (tickCount < marker.NextUpdate) {
            return;
        }

        marker.NextUpdate = tickCount + 50;

        if (marker.Spawned) {
            //actor.Field.Broadcast(FieldPacket.RemoveItem(marker.ObjectId));
        }

        marker.Spawned = true;
        //marker.ObjectId = actor.Field.RemoveMeNextLocalId();
        //
        //actor.Field.Broadcast(FieldPacket.DropDebugItem(marker.Item, marker.ObjectId, position, 1000000, 0, false));
    }

    private void RemoveDebugMarker(DebugMarker marker, long tickCount) {
        if (tickCount < marker.NextUpdate) {
            return;
        }

        if (!marker.Spawned) {
            return;
        }

        marker.NextUpdate = tickCount + 50;

        marker.Spawned = false;
        //actor.Field.Broadcast(FieldPacket.RemoveItem(marker.ObjectId));
        marker.ObjectId = 0;
    }
}
