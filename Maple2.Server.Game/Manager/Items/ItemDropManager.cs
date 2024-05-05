using Maple2.Model.Common;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;
using Maple2.Tools;

namespace Maple2.Server.Game.Manager.Items;

public class ItemDropManager {
    private readonly FieldManager field;

    public ItemDropManager(FieldManager field) {
        this.field = field;
    }

    public IList<Item> GetGlobalDropItem(int globalDropBoxId, int level = 0) {
        if (!field.ServerTableMetadata.GlobalDropItemBoxTable.DropGroups.TryGetValue(globalDropBoxId, out Dictionary<int, IList<GlobalDropItemBoxTable.Group>>? dropGroup)) {
            return new List<Item>();
        }

        IList<Item> results = new List<Item>();

        foreach ((int groupId, IList<GlobalDropItemBoxTable.Group> list) in dropGroup) {
            foreach (GlobalDropItemBoxTable.Group group in list) {
                if (!field.ServerTableMetadata.GlobalDropItemBoxTable.Items.TryGetValue(group.GroupId, out IList<GlobalDropItemBoxTable.Item>? items)) {
                    continue;
                }

                // Check if player meets level requirements.
                if (group.MinLevel > level || (group.MaxLevel > 0 && group.MaxLevel < level)) {
                    continue;
                }

                // Check map and continent conditions.
                if (group.MapTypeCondition != 0 && group.MapTypeCondition != field.Metadata.Property.Type) {
                    continue;
                }

                if (group.ContinentCondition != 0 && group.ContinentCondition != field.Metadata.Property.Continent) {
                    continue;
                }

                // Implement OwnerDrop???

                var itemsAmount = new WeightedSet<GlobalDropItemBoxTable.Group.DropCount>();
                foreach (GlobalDropItemBoxTable.Group.DropCount dropCount in group.DropCounts) {
                    itemsAmount.Add(dropCount, dropCount.Probability);
                }

                int amount = itemsAmount.Get().Amount;
                if (amount == 0) {
                    continue;
                }

                var weightedItems = new WeightedSet<GlobalDropItemBoxTable.Item>();

                // Randomize list in order to get true random items when pulled from weightedItems.
                foreach (GlobalDropItemBoxTable.Item itemEntry in items.OrderBy(i => Random.Shared.Next())) {
                    if (itemEntry.MinLevel > level || (itemEntry.MaxLevel > 0 && itemEntry.MaxLevel < level)) {
                        continue;
                    }

                    if (itemEntry.MapIds.Length > 0 && !itemEntry.MapIds.Contains(field.Metadata.Id)) {
                        continue;
                    }

                    // TODO: find quest ID and see if player has it in progress. if not, skip item. Currently just skipping.
                    if (itemEntry.QuestConstraint) {
                        continue;
                    }
                    weightedItems.Add(itemEntry, itemEntry.Weight);
                }

                if (weightedItems.Count == 0) {
                    continue;
                }

                for (int i = 0; i < amount; i++) {
                    GlobalDropItemBoxTable.Item selectedItem = weightedItems.Get();

                    int itemAmount = Random.Shared.Next(selectedItem.DropCount.Min, selectedItem.DropCount.Max + 1);
                    Item? createdItem = CreateItem(selectedItem.Id, selectedItem.Rarity, itemAmount);
                    if (createdItem == null) {
                        continue;
                    }

                    results.Add(createdItem);
                }
            }
        }

        return results;
    }

    public Item? CreateItem(int itemId, int rarity = -1, int amount = 1) {
        if (!field.ItemMetadata.TryGet(itemId, out ItemMetadata? itemMetadata)) {
            return null;
        }

        if (rarity <= 0) {
            if (itemMetadata.Option != null && itemMetadata.Option.ConstantId is < 6 and > 0) {
                rarity = itemMetadata.Option.ConstantId;
            } else {
                rarity = 1;
            }
        }

        var item = new Item(itemMetadata, rarity, amount);
        item.Stats = field.ItemStatsCalc.GetStats(item);
        item.Socket = field.ItemStatsCalc.GetSockets(item);

        if (item.Appearance != null) {
            item.Appearance.Color = GetColor(item.Metadata.Customize);
        }

        return item;
    }

    private EquipColor GetColor(ItemMetadataCustomize metadata) {
        // Item has no color
        if (metadata.ColorPalette == 0 ||
            !field.TableMetadata.ColorPaletteTable.Entries.TryGetValue(metadata.ColorPalette, out IReadOnlyDictionary<int, ColorPaletteTable.Entry>? palette)) {
            return default;
        }

        // Item has random color
        if (metadata.DefaultColorIndex < 0) {
            // random entry from palette
            int index = Random.Shared.Next(palette.Count);
            ColorPaletteTable.Entry randomEntry = palette.Values.ElementAt(index);
            return new EquipColor(randomEntry.Primary, randomEntry.Secondary, randomEntry.Tertiary, metadata.ColorPalette, index);
        }

        // Item has specified color
        if (palette.TryGetValue(metadata.DefaultColorIndex, out ColorPaletteTable.Entry? entry)) {
            return new EquipColor(entry.Primary, entry.Secondary, entry.Tertiary, metadata.ColorPalette, metadata.DefaultColorIndex);
        }

        return default;
    }
}
