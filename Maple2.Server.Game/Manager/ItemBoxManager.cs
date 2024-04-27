using System;
using System.Collections.Generic;
using System.Linq;
using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Util;

namespace Maple2.Server.Game.Manager;

public class ItemBoxManager {
    private readonly GameSession session;
    public int BoxCount { get; private set; }

    public ItemBoxManager(GameSession session) {
        this.session = session;
        BoxCount = 0;
    }

    public void Reset() {
        BoxCount = 0;
    }

    public ItemBoxError Open(Item item, int count = 1, int index = 0) {
        int[] itemBoxParams = item.Metadata.Function?.Parameters.Split(',').Select(int.Parse).ToArray() ?? Array.Empty<int>();
        return item.Metadata.Function?.Type switch {
            ItemFunction.SelectItemBox => SelectItemBox(item, itemBoxParams[0], itemBoxParams[1], index, count),
            ItemFunction.OpenItemBox => OpenItemBox(item, itemBoxParams[0], itemBoxParams[1], itemBoxParams[2], itemBoxParams[3], itemBoxParams.Length == 5 ? itemBoxParams[4] : 1, count),
            ItemFunction.OpenItemBoxWithKey => OpenItemBoxWithKey(item, itemBoxParams[0], itemBoxParams[1], itemBoxParams[2], itemBoxParams[5], count),
            ItemFunction.OpenGachaBox => OpenGachaBox(item, itemBoxParams[0], itemBoxParams[1], count),
            _ => throw new ArgumentOutOfRangeException(item.Metadata.Function?.Type.ToString(), "Invalid box type"),
        };
    }

    public ItemBoxError OpenLulluBox(Item item, int count = 1, bool autoPay = false) {
        Dictionary<string, string> parameters = XmlParseUtil.GetParameters(item.Metadata.Function?.Parameters);

        // Get common dropbox
        if (!int.TryParse(parameters["commonBoxId"], out int commonBoxId) ||
            !session.TableMetadata.IndividualItemDropTable.Entries.TryGetValue(commonBoxId, out Dictionary<byte, IList<IndividualItemDropTable.Entry>>? commonDropGroupTable)) {
            return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
        }

        // Get uncommon dropbox
        if (!int.TryParse(parameters["unCommonBoxId"], out int unCommonBoxId) ||
            !session.TableMetadata.IndividualItemDropTable.Entries.TryGetValue(unCommonBoxId, out Dictionary<byte, IList<IndividualItemDropTable.Entry>>? unCommonDropGroupTable)) {
            return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
        }

        var boxIngredient = new IngredientInfo(Enum.Parse<ItemTag>(parameters["boxItemTag"]), 1);
        var keyIngredient = new IngredientInfo(Enum.Parse<ItemTag>(parameters["keyItemTag"]), 1);
        var totalItems = new List<Item>();
        for (int startCount = 0; startCount < count; startCount++) {
            if (!session.Item.Inventory.Consume(new[] { keyIngredient })) {
                return ItemBoxError.s_err_cannot_open_multi_itembox_inventory;
            }

            if (autoPay) {
                if (!session.Item.Inventory.Consume(new[] { boxIngredient })) {
                    int.TryParse(parameters["boxPrice"], out int mesoCost);
                    mesoCost = Math.Max(mesoCost, 0);
                    if (session.Currency.CanAddMeso(-mesoCost) != -mesoCost) {
                        return ItemBoxError.s_err_cannot_open_multi_itembox_inventory;
                    }
                    session.Currency.Meso -= mesoCost;
                }
            } else {
                if (!session.Item.Inventory.Consume(new[] { boxIngredient })) {
                    return ItemBoxError.s_err_cannot_open_multi_itembox_inventory;
                }
            }

            // common dropbox items
            foreach ((byte dropGroup, IList<IndividualItemDropTable.Entry> drops) in commonDropGroupTable) {
                IList<IndividualItemDropTable.Entry> filteredDrops = session.ItemBox.FilterDrops(drops);

                // randomize contents
                IndividualItemDropTable.Entry selectedEntry = filteredDrops[Random.Shared.Next(0, filteredDrops.Count)];
                IEnumerable<Item> itemList = session.ItemBox.GetItemsFromGroup(selectedEntry);
                foreach (Item newItem in itemList) {
                    if (!session.Item.Inventory.Add(newItem, true)) {
                        session.Item.MailItem(newItem);
                    }
                }
            }

            // uncommon dropbox items
            foreach ((byte dropGroup, IList<IndividualItemDropTable.Entry> drops) in unCommonDropGroupTable) {
                IList<IndividualItemDropTable.Entry> filteredDrops = session.ItemBox.FilterDrops(drops);

                // randomize contents
                IndividualItemDropTable.Entry selectedEntry = filteredDrops[Random.Shared.Next(0, filteredDrops.Count)];
                IEnumerable<Item> itemList = session.ItemBox.GetItemsFromGroup(selectedEntry);
                foreach (Item newItem in itemList) {
                    totalItems.Add(newItem);
                    if (!session.Item.Inventory.Add(newItem, true)) {
                        session.Item.MailItem(newItem);
                    }
                }
            }
            BoxCount++;
        }

        session.Send(ItemScriptPacket.LulluBox(totalItems));
        return ItemBoxError.ok;
    }

    public ItemBoxError OpenLulluBoxSimple(Item item, int count = 1) {
        Dictionary<string, string> parameters = XmlParseUtil.GetParameters(item.Metadata.Function?.Parameters);

        // Get common dropbox
        if (!int.TryParse(parameters["commonBoxId"], out int commonBoxId) ||
            !session.TableMetadata.IndividualItemDropTable.Entries.TryGetValue(commonBoxId, out Dictionary<byte, IList<IndividualItemDropTable.Entry>>? commonDropGroupTable)) {
            return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
        }

        var keyItemTag = new IngredientInfo(Enum.Parse<ItemTag>(parameters["keyItemTag"]), 1);
        var totalItems = new List<Item>();
        for (int startCount = 0; startCount < count; startCount++) {
            if (!session.Item.Inventory.Consume(new[] { keyItemTag })) {
                return ItemBoxError.s_err_cannot_open_multi_itembox_inventory;
            }

            // common dropbox items
            foreach ((byte dropGroup, IList<IndividualItemDropTable.Entry> drops) in commonDropGroupTable) {
                IList<IndividualItemDropTable.Entry> filteredDrops = session.ItemBox.FilterDrops(drops);

                // randomize contents
                IndividualItemDropTable.Entry selectedEntry = filteredDrops[Random.Shared.Next(0, filteredDrops.Count)];
                IEnumerable<Item> itemList = session.ItemBox.GetItemsFromGroup(selectedEntry);
                foreach (Item newItem in itemList) {
                    totalItems.Add(newItem);
                    if (!session.Item.Inventory.Add(newItem, true)) {
                        session.Item.MailItem(newItem);
                    }
                }
            }

            BoxCount++;
        }

        session.Send(ItemScriptPacket.LulluBox(totalItems));
        return ItemBoxError.ok;
    }

    private ItemBoxError SelectItemBox(Item item, int itemRequiredAmount, int boxId, int index, int count = 1) {
        // check if box count is enough to open
        if (session.Item.Inventory.Find(item.Id).Sum(box => box.Amount) < itemRequiredAmount) {
            return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
        }

        // check if we can get metadata
        if (!session.TableMetadata.IndividualItemDropTable.Entries.TryGetValue(boxId, out Dictionary<byte, IList<IndividualItemDropTable.Entry>>? dropGroupTable)) {
            return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
        }

        // SelectItemBox disregards DropGroup
        IList<IndividualItemDropTable.Entry> drops = session.ItemBox.FilterDrops(dropGroupTable.Values.SelectMany(x => x).ToList());


        IndividualItemDropTable.Entry? selectedEntry = drops.ElementAtOrDefault(index);
        if (selectedEntry == null) {
            return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
        }

        var error = ItemBoxError.ok;
        ItemComponent ingredient = new(item.Id, -1, itemRequiredAmount, ItemTag.None);
        for (int startCount = 0; startCount < count; startCount++) {
            if (!session.Item.Inventory.ConsumeItemComponents(new[] { ingredient })) {
                return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
            }

            IEnumerable<Item> itemList = session.ItemBox.GetItemsFromGroup(selectedEntry);
            foreach (Item newItem in itemList) {
                if (!session.Item.Inventory.Add(newItem, true)) {
                    session.Item.MailItem(newItem);
                    error = ItemBoxError.s_err_cannot_open_multi_itembox_inventory;
                }
            }
            BoxCount++;
            if (error != ItemBoxError.ok) {
                return error;
            }
        }
        return error;
    }

    private ItemBoxError OpenItemBox(Item item, int globalDropBoxId, int unknownId, int itemId, int boxId, int itemRequiredAmount, int count = 1) {
        // if globalDropBoxId > 0, unknownId is possibly another globalDropBoxId. Not confirmed, just a guess.
        // otherwise unknownId is treated as a bool. if 1 = receive one item from each drop group, if 0 = receive all items from one drop group

        if (boxId == 0) {
            return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
        }

        if (itemRequiredAmount > -1 && session.Item.Inventory.Find(item.Id).Sum(box => box.Amount) < itemRequiredAmount) {
            return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
        }

        // TODO: branch off to globalDropBoxId. Temp solution
        if (globalDropBoxId > 0) {
            return HandleGlobalDropBox(item, itemId, itemRequiredAmount, count);
        }

        // Get dropbox
        if (!session.TableMetadata.IndividualItemDropTable.Entries.TryGetValue(boxId, out Dictionary<byte, IList<IndividualItemDropTable.Entry>>? dropGroupTable)) {
            return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
        }

        var error = ItemBoxError.ok;
        if (unknownId == 1) {
            ItemComponent ingredient = new(item.Id, item.Rarity, itemRequiredAmount, ItemTag.None);
            for (int startCount = 0; startCount < count; startCount++) {
                if (!session.Item.Inventory.ConsumeItemComponents(new[] { ingredient })) {
                    return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
                }

                if (itemId > 0) {
                    Item? newItem = session.Item.CreateItem(itemId, item.Metadata.Option?.ConstantId ?? 1);
                    if (newItem == null) {
                        continue;
                    }
                    if (!session.Item.Inventory.Add(newItem, true)) {
                        session.Item.MailItem(newItem);
                        error = ItemBoxError.s_err_cannot_open_multi_itembox_inventory;
                    }
                }

                foreach ((byte dropGroup, IList<IndividualItemDropTable.Entry> drops) in dropGroupTable) {
                    IList<IndividualItemDropTable.Entry> filteredDrops = session.ItemBox.FilterDrops(drops);

                    // randomize contents
                    IndividualItemDropTable.Entry selectedEntry = filteredDrops[Random.Shared.Next(0, filteredDrops.Count)];
                    IEnumerable<Item> itemList = session.ItemBox.GetItemsFromGroup(selectedEntry);
                    foreach (Item newItem in itemList) {
                        if (!session.Item.Inventory.Add(newItem, true)) {
                            session.Item.MailItem(newItem);
                            error = ItemBoxError.s_err_cannot_open_multi_itembox_inventory;
                        }
                    }
                }
                BoxCount++;
                if (error != ItemBoxError.ok) {
                    return error;
                }
            }
        } else {
            error = GiveAllDropBoxItems(item, itemId, itemRequiredAmount, count, dropGroupTable);
        }
        return error;
    }
    private ItemBoxError GiveAllDropBoxItems(Item item, int itemId, int itemRequiredAmount, int count, Dictionary<byte, IList<IndividualItemDropTable.Entry>> dropGroupTable) {
        var error = ItemBoxError.ok;
        // get every item in every drop group
        IList<IndividualItemDropTable.Entry> drops = session.ItemBox.FilterDrops(dropGroupTable.Values.SelectMany(x => x).ToList());
        ItemComponent ingredient = new(item.Id, item.Rarity, itemRequiredAmount, ItemTag.None);
        for (int startCount = 0; startCount < count; startCount++) {
            if (!session.Item.Inventory.ConsumeItemComponents(new[] { ingredient })) {
                return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
            }

            if (itemId > 0) {
                Item? newItem = session.Item.CreateItem(itemId);
                if (newItem == null) {
                    continue;
                }
                if (!session.Item.Inventory.Add(newItem, true)) {
                    session.Item.MailItem(newItem);
                    error = ItemBoxError.s_err_cannot_open_multi_itembox_inventory;
                }
            }
            foreach (IndividualItemDropTable.Entry drop in drops) {
                IEnumerable<Item> itemList = session.ItemBox.GetItemsFromGroup(drop);
                foreach (Item newItem in itemList) {
                    if (!session.Item.Inventory.Add(newItem, true)) {
                        session.Item.MailItem(newItem);
                        error = ItemBoxError.s_err_cannot_open_multi_itembox_inventory;
                    }
                }
            }
            BoxCount++;
            if (error != ItemBoxError.ok) {
                return error;
            }
        }
        return error;
    }

    private ItemBoxError OpenItemBoxWithKey(Item item, int keyItemId, int keyAmountRequired, int itemId, int boxId, int count = 1) {
        if (keyItemId == 0) {
            return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
        }

        // check for keys
        if (session.Item.Inventory.Find(keyItemId).Sum(requiredItem => requiredItem.Amount) < keyAmountRequired) {
            return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
        }

        // Get dropbox
        if (!session.TableMetadata.IndividualItemDropTable.Entries.TryGetValue(boxId, out Dictionary<byte, IList<IndividualItemDropTable.Entry>>? dropGroupTable)) {
            return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
        }
        return GiveAllDropBoxItems(item, itemId, keyAmountRequired, count, dropGroupTable);
    }

    private ItemBoxError OpenGachaBox(Item item, int gachaBoxId, int addItemId, int count) {
        if (!session.TableMetadata.GachaInfoTable.Entries.TryGetValue(gachaBoxId, out GachaInfoTable.Entry? gachaEntry)) {
            return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
        }

        if (!session.TableMetadata.IndividualItemDropTable.Entries.TryGetValue(gachaEntry.DropBoxId, out Dictionary<byte, IList<IndividualItemDropTable.Entry>>? dropGroupTable)) {
            return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
        }

        var totalItems = new List<Item>();
        ItemComponent ingredient = new(item.Id, item.Rarity, 1, ItemTag.None);
        for (int startCount = 0; startCount < count; startCount++) {
            if (!session.Item.Inventory.ConsumeItemComponents(new[] { ingredient })) {
                return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
            }

            // This is only applicable if Feature "ItemCN" is enabled
            /*Item? additionalItem = session.Item.CreateItem(addItemId);
            if (additionalItem != null && !session.Item.Inventory.Add(additionalItem, true)) {
                session.Item.MailItem(additionalItem);
            }*/

            foreach ((byte dropGroup, IList<IndividualItemDropTable.Entry> drops) in dropGroupTable) {
                IList<IndividualItemDropTable.Entry> filteredDrops = session.ItemBox.FilterDrops(drops);

                // randomize contents
                IndividualItemDropTable.Entry selectedEntry = filteredDrops[Random.Shared.Next(0, filteredDrops.Count)];
                IEnumerable<Item> itemList = session.ItemBox.GetItemsFromGroup(selectedEntry);
                foreach (Item newItem in itemList) {
                    newItem.GachaDismantleId = gachaBoxId;
                    totalItems.Add(newItem);
                    if (!session.Item.Inventory.Add(newItem, true)) {
                        session.Item.MailItem(newItem);
                    }

                    // TODO: Implement a weight system, possibly determined by GachaInfoTable.Entry.RandomBoxGroup ??
                    // TODO: Broadcast server message if item rarity >= 4
                }
            }
            BoxCount++;
        }

        session.Send(ItemScriptPacket.Gacha(totalItems));
        return ItemBoxError.ok;
    }

    private IList<IndividualItemDropTable.Entry> FilterDrops(IEnumerable<IndividualItemDropTable.Entry> entries) {
        var filteredEntries = new List<IndividualItemDropTable.Entry>();
        foreach (IndividualItemDropTable.Entry entry in entries) {
            if (!session.ItemMetadata.TryGet(entry.ItemIds[0], out ItemMetadata? itemMetadata)) {
                continue;
            }

            // Check for any job restrictions
            if (entry.SmartDropRate > 0 && itemMetadata.Limit.JobRecommends.Length > 0 &&
                !itemMetadata.Limit.JobRecommends.Contains(session.Player.Value.Character.Job.Code()) && !itemMetadata.Limit.JobRecommends.Contains(JobCode.None)) {
                continue;
            }

            if (entry.SmartGender && itemMetadata.Limit.Gender != Gender.All && itemMetadata.Limit.Gender != session.Player.Value.Character.Gender) {
                continue;
            }
            filteredEntries.Add(entry);
        }
        return filteredEntries;
    }

    private IEnumerable<Item> GetItemsFromGroup(IndividualItemDropTable.Entry entry) {
        var items = new List<Item>();
        foreach (int id in entry.ItemIds) {
            Item? newItem = session.Item.CreateItem(id, amount: Random.Shared.Next(entry.MinCount, entry.MaxCount));
            if (newItem == null) {
                continue;
            }

            if (entry.ReduceRepackLimit && newItem.Transfer?.RepackageCount > 0) {
                newItem.Transfer.RepackageCount++;
            }

            // TODO: Apply enchant level & stats

            if (entry.ReduceTradeCount && newItem.Transfer?.RemainTrades > 0) {
                newItem.Transfer.RemainTrades--;
            }

            if (entry.Bind) {
                newItem.Transfer?.Bind(session.Player.Value.Character);
            }
            items.Add(newItem);
        }
        return items;
    }

    private ItemBoxError HandleGlobalDropBox(Item item, int itemId, int itemRequiredAmount, int count) {
        var error = ItemBoxError.ok;
        ItemComponent ingredient = new(item.Id, item.Rarity, itemRequiredAmount, ItemTag.None);
        for (int startCount = 0; startCount < count; startCount++) {
            // assumes itemBoxParams[1] is another globalDropBoxId utilized
            if (!session.Item.Inventory.ConsumeItemComponents(new[] { ingredient })) {
                error = ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
            }

            if (itemId > 0) {
                Item? newItem = session.Item.CreateItem(itemId);
                if (newItem == null) {
                    continue;
                }
                if (!session.Item.Inventory.Add(newItem, true)) {
                    session.Item.MailItem(newItem);
                    error = ItemBoxError.s_err_cannot_open_multi_itembox_inventory;
                }
                BoxCount++;
                if (error != ItemBoxError.ok) {
                    return error;
                }
            }
        }
        // return fail error for now until globalDropBox is implemented
        return ItemBoxError.s_err_cannot_open_multi_itembox_inventory_fail;
    }
}
