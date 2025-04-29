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
    public int[] Distributions { get; } = [];
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
        Distributions = CalculateDistributions(ItemCount, MaxUserCount);
    }

    private int[] CalculateDistributions(int totalAmount, int remainingRecipients) {
        if (remainingRecipients <= 0) return [];
        if (remainingRecipients == 1) return [totalAmount];

        // Ensure minimum 1 meret per person
        int remainingMeret = totalAmount - remainingRecipients;

        // Calculate maximum possible amount for this person
        // Leave enough for others to get at least 1 each
        int maxPossible = remainingMeret - (remainingRecipients - 1);

        // Generate random amount between 1 and maxPossible
        int amount = Random.Shared.Next(1, Math.Max(2, maxPossible + 1));

        // Recursively distribute the rest
        int[] remaining = CalculateDistributions(
            totalAmount - amount,
            remainingRecipients - 1
        );

        return new[] {
            amount
        }.Concat(remaining).ToArray();
    }

    public void Claim(FieldPlayer player) {
        if (Players.Count >= MaxUserCount) {
            return;
        }

        if (Players.ContainsKey(player.ObjectId)) {
            return;
        }

        if (!Players.TryAdd(player.ObjectId, player)) {
            return;
        }

        int distributionIndex = Players.Count - 1;
        if (distributionIndex < 0 || distributionIndex >= Distributions.Length) {
            return;
        }
        Item? item = player.Session.Field.ItemDrop.CreateItem(ItemId, amount: Distributions[distributionIndex]);
        if (item == null) {
            return;
        }

        player.Session.Send(PlayerHostPacket.GiftHongBao(player, this, item.Amount));
        if (!player.Session.Item.Inventory.Add(item, true)) {
            player.Session.Item.MailItem(item);
        }
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
