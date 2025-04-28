using System.Collections.Concurrent;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Packets;
using Maple2.Tools;

namespace Maple2.Server.Game.Model;

public class HongBao : IUpdatable, IByteSerializable {
    private readonly FieldManager field;
    public int ObjectId { get; init; }
    public int SourceItemId { get; init; }
    public FieldPlayer Owner { get; init; }
    public ConcurrentDictionary<int, FieldPlayer> Players { get; } = new();
    public int MaxUserCount { get; init; }
    public long EndTick { get; }
    public int ItemId { get; }
    public int ItemCount { get; }
    private bool active;

    public HongBao(FieldManager field, FieldPlayer owner, int sourceItemId, int objectId, int itemId, int maxUserCount, long startTick, int durationSec, int itemCount) {
        ObjectId = objectId;
        this.field = field;
        SourceItemId = sourceItemId;
        Owner = owner;
        MaxUserCount = maxUserCount;
        EndTick = startTick + (long) TimeSpan.FromSeconds(durationSec).TotalMilliseconds;
        ItemId = itemId;
        ItemCount = itemCount;
        active = true;
        Players.TryAdd(owner.ObjectId, owner);
    }

    public bool AddPlayer(FieldPlayer player) {
        if (Players.Count >= MaxUserCount) {
            return false;
        }

        if (Players.ContainsKey(player.ObjectId)) {
            return false;
        }

        Players.TryAdd(player.ObjectId, player);
        return true;
    }

    public void Update(long tickCount) {
        if (tickCount < EndTick && Players.Count < MaxUserCount) {
            return;
        }
        if (!active) {
            return;
        }
        active = false;
        field.RemoveHongBao(ObjectId);
    }
    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(SourceItemId);
        writer.WriteInt(ObjectId);
        writer.WriteInt(ItemId);
        writer.WriteInt(ItemCount);
        writer.WriteInt(MaxUserCount);
        writer.WriteUnicodeString(Owner.Value.Character.Name);
    }
}
