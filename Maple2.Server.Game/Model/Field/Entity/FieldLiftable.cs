using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Packets;

namespace Maple2.Server.Game.Model;

public class FieldLiftable : FieldEntity<Liftable> {
    public readonly string EntityId;

    public int Count;
    public LiftableState State;
    public long FinishTick;

    public long RespawnTick { get; private set; }

    public FieldLiftable(FieldManager field, int objectId, string entityId, Liftable value) : base(field, objectId, value) {
        EntityId = entityId;
        Count = Value.ItemStackCount;
        FinishTick = Value.ItemLifetime > 0 ? Environment.TickCount64 + Value.ItemLifetime : 0;
    }

    public LiftableCube? Pickup() {
        if (Count <= 0 || State != LiftableState.Default) {
            return null;
        }

        Count--;
        // Only respawn if we have a regen time
        if (RespawnTick == 0 && Value.RegenCheckTime > 0) {
            RespawnTick = Environment.TickCount64 + Value.RegenCheckTime;
        }

        if (Count > 0) {
            Field.Broadcast(LiftablePacket.Update(this));
        } else {
            State = LiftableState.Respawning;
            Field.Broadcast(LiftablePacket.Remove(EntityId));
            Field.Broadcast(CubePacket.RemoveCube(ObjectId, Position));
        }

        return new LiftableCube(Value);
    }

    public override void Update(long tickCount) {
        // Handles despawning after being placed
        if (FinishTick != 0 && tickCount > FinishTick) {
            Field.RemoveLiftable(EntityId);
            return;
        }

        if (RespawnTick == 0 || tickCount < RespawnTick) {
            return;
        }

        Count++;
        // Only respawn if we have a regen time
        if (Count < Value.ItemStackCount && Value.RegenCheckTime > 0) {
            RespawnTick = tickCount + Value.RegenCheckTime;
        } else {
            RespawnTick = 0;
        }

        if (Count == 1) {
            State = LiftableState.Default;
            Field.Broadcast(LiftablePacket.Add(this));
            Field.Broadcast(CubePacket.PlaceLiftable(ObjectId, new LiftableCube(Value), Position, Rotation.Z));
        }
        Field.Broadcast(LiftablePacket.Update(this));
    }
}
