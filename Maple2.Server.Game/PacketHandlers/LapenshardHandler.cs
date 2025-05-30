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

public class LapenshardHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.Lapenshard;

    private enum Command : byte {
        Equip = 1,
        Unequip = 2,
        AddLapenshard = 3,
        AddFodder = 4,
        Upgrade = 5,
        Unknown = 6,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required TableMetadataStorage TableMetadata { private get; init; }
    public required ItemMetadataStorage ItemMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Equip:
                HandleEquip(session, packet);
                return;
            case Command.Unequip:
                HandleUnequip(session, packet);
                return;
            case Command.AddLapenshard:
                HandleAddLapenshard(session, packet);
                return;
            case Command.AddFodder:
                HandleAddFodder(session, packet);
                return;
            case Command.Upgrade:
                HandleUpgrade(session, packet);
                return;
            case Command.Unknown:
                HandleUnknown(session, packet);
                return;
        }
    }

    private static void HandleEquip(GameSession session, IByteReader packet) {
        var slot = packet.Read<LapenshardSlot>();
        long itemUid = packet.ReadLong();

        session.Config.EquipLapenshard(itemUid, slot);
    }

    private static void HandleUnequip(GameSession session, IByteReader packet) {
        var slot = packet.Read<LapenshardSlot>();

        session.Config.UnequipLapenshard(slot);
    }

    private static void HandleAddLapenshard(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        int itemId = packet.ReadInt();
        var slot = packet.Read<LapenshardSlot>();

        if (GetLapenshardId(session, itemUid, slot) != itemId) {
            return;
        }

        session.Send(LapenshardPacket.Preview());
    }

    private static void HandleAddFodder(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        int itemId = packet.ReadInt();
        packet.Read<LapenshardSlot>(); // You shouldn't be able to add an equip as fodder.
        int amount = packet.ReadInt();

        Item? item = session.Item.Inventory.Get(itemUid, InventoryType.Lapenshard);
        if (item == null || item.Id != itemId || item.Amount < amount) {
            return;
        }

        session.Send(LapenshardPacket.Preview());
    }

    private void HandleUpgrade(GameSession session, IByteReader packet) {
        long lapenshardUid = packet.ReadLong();
        int lapenshardId = packet.ReadInt();
        var slot = packet.Read<LapenshardSlot>();

        var fodders = new Dictionary<long, int>();
        int count = packet.ReadInt();
        for (int i = 0; i < count; i++) {
            long fodderUid = packet.ReadLong(); // Uid
            int fodderAmount = packet.ReadInt(); // Amount
            fodders[fodderUid] = fodderAmount;
        }

        if (!TableMetadata.LapenshardUpgradeTable.Entries.TryGetValue(lapenshardId, out LapenshardUpgradeTable.Entry? entry)) {
            return;
        }
        if (fodders.Values.Sum() != entry.RequireCount) {
            return;
        }

        lock (session.Item) {
            if (GetLapenshardId(session, lapenshardUid, slot) != lapenshardId) {
                return;
            }
            foreach ((long uid, int amount) in fodders) {
                Item? fodder = session.Item.Inventory.Get(uid, InventoryType.Lapenshard);
                if (fodder == null || fodder.Amount < amount) {
                    return; // Invalid amount for fodder
                }
                if (!TableMetadata.LapenshardUpgradeTable.Entries.TryGetValue(fodder.Id, out LapenshardUpgradeTable.Entry? fodderEntry)) {
                    return; // Fodder is not a valid lapenshard
                }
                if (fodderEntry.GroupId != entry.GroupId) {
                    return; // Fodder group does not match lapenshard being upgraded
                }
            }
            if (session.Currency.CanAddMeso(-entry.Meso) != -entry.Meso) {
                return;
            }

            if (lapenshardUid > 0) {
                if (UpgradeInventoryLapenshard(session, lapenshardUid, fodders, entry)) {
                    session.Send(LapenshardPacket.Upgrade(lapenshardUid, lapenshardId, slot));
                }
            } else {
                if (UpgradeEquipLapenshard(session, slot, fodders, entry)) {
                    session.Config.LoadLapenshard();
                    session.Send(LapenshardPacket.Upgrade(lapenshardUid, lapenshardId, slot));
                }
            }
        }
    }

    // Gets the ItemId of the Lapenshard from either Inventory or Equip.
    private static int GetLapenshardId(GameSession session, long uid, LapenshardSlot slot) {
        if (uid > 0) {
            return session.Item.Inventory.Get(uid, InventoryType.Lapenshard)?.Id ?? -1;
        }
        return session.Config.TryGetLapenshard(slot);
    }

    private bool UpgradeInventoryLapenshard(GameSession session, long lapenshardUid, Dictionary<long, int> fodders, LapenshardUpgradeTable.Entry entry) {
        if (!ItemMetadata.TryGet(entry.NextItemId, out ItemMetadata? nextItem)) {
            return false;
        }

        IngredientInfo[] ingredients = entry.Ingredients
            .Select(ingredient => new IngredientInfo(ingredient.ItemTag, ingredient.Amount))
            .ToArray();
        if (!session.Item.Inventory.Consume(ingredients)) {
            return false;
        }
        foreach ((long uid, int amount) in fodders) {
            if (!session.Item.Inventory.Consume(uid, amount)) {
                throw new InvalidOperationException($"Failed to consume fodder: {uid} after validating");
            }
        }
        session.Currency.Meso -= entry.Meso;

        if (!session.Item.Inventory.Remove(lapenshardUid, out Item? lapenshard, 1)) {
            throw new InvalidOperationException($"Failed to remove lapenshard: {lapenshardUid} after validating");
        }

        Item upgradeLapenshard = lapenshard.Mutate(nextItem, Constant.LapenshardGrade);
        if (!session.Item.Inventory.Add(upgradeLapenshard)) {
            Logger.Fatal("Failed to add upgraded lapenshard {ItemUid} to inventory", upgradeLapenshard.Uid);
            return false;
        }
        return true;
    }

    private static bool UpgradeEquipLapenshard(GameSession session, LapenshardSlot slot, Dictionary<long, int> fodders, LapenshardUpgradeTable.Entry entry) {
        IngredientInfo[] ingredients = entry.Ingredients
            .Select(ingredient => new IngredientInfo(ingredient.ItemTag, ingredient.Amount))
            .ToArray();
        if (!session.Item.Inventory.Consume(ingredients)) {
            return false;
        }
        foreach ((long uid, int amount) in fodders) {
            if (!session.Item.Inventory.Consume(uid, amount)) {
                throw new InvalidOperationException($"Failed to consume fodder: {uid} after validating");
            }
        }
        session.Currency.Meso -= entry.Meso;
        session.Config.SetLapenshard(slot, entry.NextItemId);
        return true;
    }

    private static void HandleUnknown(GameSession session, IByteReader packet) {
        int itemId = packet.ReadInt();
    }
}
