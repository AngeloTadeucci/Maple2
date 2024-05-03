using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Session;
using Maple2.Tools;

namespace Maple2.Server.Game.Manager.Items;

public class ItemDropManager {
    private readonly GameSession session;

    public ItemDropManager(GameSession session) {
        this.session = session;
    }

    public IList<Item> GetGlobalDropItem(int globalDropBoxId, short level) {
        if (!session.ServerTableMetadata.GlobalDropItemBoxTable.DropGroups.TryGetValue(globalDropBoxId, out Dictionary<int, IList<GlobalDropItemBoxTable.Group>>? dropGroup)) {
            return new List<Item>();

        }

        IList<Item> results = new List<Item>();

        foreach ((int groupId, IList<GlobalDropItemBoxTable.Group> list) in dropGroup) {
            foreach (GlobalDropItemBoxTable.Group group in list) {
                if (!session.ServerTableMetadata.GlobalDropItemBoxTable.Items.TryGetValue(group.GroupId, out IList<GlobalDropItemBoxTable.Item>? items)) {
                    continue;
                }

                // Check if player meets level requirements.
                if (group.MinLevel > level || (group.MaxLevel > 0 && group.MaxLevel < level)) {
                    continue;
                }

                // Check map and continent conditions.
                if (group.MapTypeCondition != 0 && group.MapTypeCondition != session.Player.Field.Metadata.Property.Type) {
                    continue;
                }

                if (group.ContinentCondition != 0 && group.ContinentCondition != session.Player.Field.Metadata.Property.Continent) {
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
                    if (itemEntry.MinLevel > level || itemEntry.MaxLevel < level) {
                       continue;
                    }

                    if (itemEntry.MapIds.Length > 0 && !itemEntry.MapIds.Contains(session.Player.Field.Metadata.Id)) {
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

                    Item? createdItem = session.Item.CreateItem(selectedItem.Id, selectedItem.Rarity, itemAmount);
                    if (createdItem == null) {
                        continue;
                    }
                    results.Add(createdItem);
                }
            }
        }

        return results;
    }
}
