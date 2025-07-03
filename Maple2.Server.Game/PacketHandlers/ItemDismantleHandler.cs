using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class ItemDismantleHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.ItemDismantle;

    private enum Command : byte {
        Interface = 0,
        Stage = 1,
        Remove = 2,
        Confirm = 3,
        AutoAdd = 6,
    }

    // TODO: Simple calculation now of 10k onyx per item
    private const int ONYX_ID = 40100023;
    private const int MIN_ONYX = 700;
    private const int MAX_ONYX = 1000;

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required ItemMetadataStorage ItemMetadata { private get; init; }
    public required TableMetadataStorage TableMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Interface:
                HandleInterface(session, packet);
                return;
            case Command.Stage:
                HandleStage(session, packet);
                return;
            case Command.Remove:
                HandleRemove(session, packet);
                return;
            case Command.Confirm:
                HandleConfirm(session);
                return;
            case Command.AutoAdd:
                HandleAutoAdd(session, packet);
                return;
        }
    }

    private void HandleInterface(GameSession session, IByteReader packet) {
        session.DismantleOpened = packet.ReadBool();
        Array.Clear(session.DismantleStaging);
    }

    private void HandleStage(GameSession session, IByteReader packet) {
        if (!session.DismantleOpened) {
            return;
        }

        packet.ReadInt(); // -1
        long itemUid = packet.ReadLong();
        int amount = packet.ReadInt();
        Item? item = session.Item.Inventory.Get(itemUid);
        if (item == null || (!item.Metadata.Limit.EnableBreak && item.GachaDismantleId == 0) || item.Amount < amount) {
            return;
        }

        for (short slot = 0; slot < session.DismantleStaging.Length; slot++) {
            if (session.DismantleStaging[slot] != default) {
                if (session.DismantleStaging[slot].Uid == itemUid) {
                    return;
                }
                continue;
            }

            session.DismantleStaging[slot] = (item.Uid, amount);
            session.Send(ItemDismantlePacket.Stage(itemUid, slot, amount));
            break;
        }

        session.Send(ItemDismantlePacket.Preview(GetRewards(session)));
    }

    private void HandleRemove(GameSession session, IByteReader packet) {
        if (!session.DismantleOpened) {
            return;
        }

        long itemUid = packet.ReadLong();
        for (short slot = 0; slot < session.DismantleStaging.Length; slot++) {
            if (session.DismantleStaging[slot].Uid != itemUid) {
                continue;
            }

            session.DismantleStaging[slot] = default;
            session.Send(ItemDismantlePacket.Remove(itemUid));
            break;
        }

        session.Send(ItemDismantlePacket.Preview(GetRewards(session)));
    }

    private void HandleConfirm(GameSession session) {
        if (!session.DismantleOpened) {
            return;
        }

        // TODO: Confirm inventory can hold all the items.
        lock (session.Item) {
            var rewards = new Dictionary<int, (int Min, int Max)>();
            for (short slot = 0; slot < session.DismantleStaging.Length; slot++) {
                (long uid, int amount) = session.DismantleStaging[slot];
                Item? item = session.Item.Inventory.Get(uid);
                if (item == null) {
                    continue;
                }

                // If failed to consume, remove slot.
                if (!session.Item.Inventory.Consume(uid, amount)) {
                    session.DismantleStaging[slot] = default;
                    continue;
                }

                foreach ((int id, int min, int max) in GetReward(item, amount)) {
                    (int Min, int Max) prev = rewards.GetValueOrDefault(id, (0, 0));
                    rewards[id] = (prev.Min + min, prev.Max + max);
                }
            }

            Array.Clear(session.DismantleStaging);

            var result = new Dictionary<int, int>();
            foreach ((int id, (int min, int max)) in rewards) {
                int total = Random.Shared.Next(min, max);
                result[id] = total;
                while (total > 0) {

                    Item? item = session.Field?.ItemDrop.CreateItem(id, amount: total);
                    if (item == null) {
                        continue;
                    }
                    int rewarded = Math.Min(item.Metadata.Property.SlotMax, total);
                    total -= rewarded;
                    session.Item.Inventory.Add(item, true);
                }
            }

            session.Send(ItemDismantlePacket.Result(result));
        }
    }

    private void HandleAutoAdd(GameSession session, IByteReader packet) {
        if (!session.DismantleOpened) {
            return;
        }

        var type = packet.Read<InventoryType>();
        byte rarity = packet.ReadByte();

        IList<Item> items = session.Item.Inventory.Filter(item => item.Rarity <= rarity, type);
        for (short slot = 0, i = 0; slot < session.DismantleStaging.Length && i < items.Count; slot++) {
            if (session.DismantleStaging[slot] != default) {
                continue;
            }

            while (i < items.Count) {
                Item item = items[i++];
                if (session.DismantleStaging.Any(existing => existing.Uid == item.Uid)) {
                    continue;
                }

                if (item.IsLocked) {
                    continue;
                }

                switch (type) {
                    case InventoryType.Gear or InventoryType.Gemstone:
                        if (!item.Metadata.Limit.EnableBreak) {
                            continue;
                        }
                        break;
                    case InventoryType.Outfit:
                        if (item.GachaDismantleId == 0) {
                            continue;
                        }
                        break;
                }

                session.DismantleStaging[slot] = (item.Uid, item.Amount);
                session.Send(ItemDismantlePacket.Stage(item.Uid, slot, item.Amount));
                break;
            }
        }

        session.Send(ItemDismantlePacket.Preview(GetRewards(session)));
    }

    private IDictionary<int, (int Min, int Max)> GetRewards(GameSession session) {
        var rewards = new Dictionary<int, (int Min, int Max)>();
        foreach ((long uid, int amount) in session.DismantleStaging) {
            Item? item = session.Item.Inventory.Get(uid);
            if (item == null || item.Amount < amount) {
                continue;
            }

            foreach ((int id, int min, int max) in GetReward(item, amount)) {
                (int Min, int Max) prev = rewards.GetValueOrDefault(id, (0, 0));
                rewards[id] = (prev.Min + min, prev.Max + max);
            }
        }

        return rewards;
    }

    private IEnumerable<(int Id, int Min, int Max)> GetReward(Item item, int amount) {
        if (amount <= 0 || item.Amount < amount) {
            yield break;
        }

        if (TableMetadata.ItemBreakTable.Entries.TryGetValue(item.Id, out IReadOnlyList<ItemBreakTable.Ingredient>? ingredients)) {
            foreach (ItemBreakTable.Ingredient ingredient in ingredients) {
                yield return (ingredient.ItemId, ingredient.Amount * amount, ingredient.Amount * amount);
            }
        } else if (item.GachaDismantleId != 0 && TableMetadata.GachaInfoTable.Entries.TryGetValue(item.GachaDismantleId, out GachaInfoTable.Entry? gachaEntry)) {
            yield return (gachaEntry.CoinItemId, gachaEntry.CoinItemAmount, gachaEntry.CoinItemAmount);
        } else {
            // TODO: Calculate onyx to reward from item rarity/enchant
            int minAdd = MIN_ONYX * item.Rarity * amount;
            int maxAdd = MAX_ONYX * item.Rarity * amount;
            yield return (ONYX_ID, minAdd, maxAdd);
        }
        // TODO: ItemSkinCrystal reward from outfits?
    }
}
