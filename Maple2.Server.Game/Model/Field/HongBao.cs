using System.Collections.Concurrent;
using Maple2.Model.Game;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Model;

public class HongBao : IUpdatable {
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
            amount,
        }.Concat(remaining).ToArray();
    }

    public Item? Claim(FieldPlayer player) {
        if (Players.Count >= MaxUserCount) {
            return null;
        }

        if (Players.ContainsKey(player.ObjectId)) {
            return null;
        }

        if (!Players.TryAdd(player.ObjectId, player)) {
            return null;
        }

        int distributionIndex = Players.Count - 1;
        if (distributionIndex < 0 || distributionIndex >= Distributions.Length) {
            return null;
        }
        return player.Session.Field.ItemDrop.CreateItem(ItemId, amount: Distributions[distributionIndex]);
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
}
