﻿using System;
using System.Collections.Generic;
using System.Linq;
using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public Item? CreateItem(long ownerId, Item item) {
            Model.Item model = item!;
            model.OwnerId = ownerId;
            model.Id = 0;
            Context.Item.Add(model);

            return Context.TrySaveChanges() ? ToItem(model) : null;
        }

        public Item? SplitItem(long ownerId, Item item, int amount) {
            Model.Item model = item!;
            model.Amount = amount;
            model.OwnerId = ownerId;
            model.Slot = -1;
            model.EquipSlot = EquipSlot.Unknown;
            model.EquipTab = EquipTab.None;
            model.Id = 0;
            Context.Item.Add(model);

            return Context.TrySaveChanges() ? ToItem(model) : null;
        }

        public List<Item>? CreateItems(long ownerId, params Item[] items) {
            var models = new Model.Item[items.Length];
            for (int i = 0; i < items.Length; i++) {
                models[i] = items[i]!;
                models[i].OwnerId = ownerId;
                models[i].Id = 0;
                Context.Item.Add(models[i]);
            }

            if (!Context.TrySaveChanges()) {
                return null;
            }

            return models.Select(ToItem).Where(item => item != null).ToList()!;
        }

        public Item? GetItem(long itemUid) {
            Model.Item? model = Context.Item.Find(itemUid);
            if (model == null) {
                return null;
            }

            return game.itemMetadata.TryGet(model.ItemId, out ItemMetadata? metadata) ? model.Convert(metadata) : null;
        }

        public IDictionary<EquipTab, List<Item>> GetEquips(long characterId, params EquipTab[] tab) {
            return Context.Item.Where(item => item.OwnerId == characterId && tab.Contains(item.EquipTab))
                .AsEnumerable()
                .GroupBy(item => item.EquipTab)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(ToItem).Where(item => item != null).ToList()
                )!;
        }

        public Dictionary<InventoryType, List<Item>> GetInventory(long characterId) {
            return Context.Item.Where(item => item.OwnerId == characterId && item.EquipTab == EquipTab.None)
                .AsEnumerable()
                .Select(ToItem)
                .Where(item => item != null)
                .GroupBy(item => item!.Inventory)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList()
                )!;
        }

        public List<Item> GetItems(long characterId) {
            return Context.Item.Where(item => item.OwnerId == characterId)
                .AsEnumerable()
                .Select(ToItem)
                .Where(item => item != null)
                .ToList()!;
        }

        public bool SaveItems(long ownerId, params Item[] items) {
            var models = new Model.Item[items.Length];
            for (int i = 0; i < items.Length; i++) {
                if (items[i].Uid == 0) {
                    continue;
                }

                models[i] = items[i]!;
                models[i].OwnerId = ownerId;
                Context.Item.Update(models[i]);
            }

            return Context.TrySaveChanges();
        }

        // Converts model to item if possible, otherwise returns null.
        private Item? ToItem(Model.Item? model) {
            if (model == null) {
                return null;
            }

            return game.itemMetadata.TryGet(model.ItemId, out ItemMetadata? metadata) ? model.Convert(metadata) : null;
        }
    }
}
