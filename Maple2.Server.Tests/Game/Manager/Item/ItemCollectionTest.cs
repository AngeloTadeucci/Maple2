using System;
using System.Collections.Generic;
using System.Linq;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Items;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Maple2.Server.Tests.Game.Manager.Item;

[TestClass]
public class ItemCollectionTest {
    private static readonly Random Rng = new(123);

    [TestMethod]
    public void AddSlotWithIndexer() {
        var item = CreateItem(1000);
        var otherItem = CreateItem(1000);
        var collection = new ItemCollection(1) {
            [0] = item,
        };
        Assert.AreEqual(item, collection[0]);
        Assert.AreEqual(0, item.Slot);
        Assert.AreEqual(item, collection.Get(item.Uid));

        collection[0] = null; // Not allowed, NOP
        Assert.AreEqual(collection[0], item);
        collection[0] = otherItem; // Slot taken, NOP
        Assert.AreEqual(collection[0], item);

        // Invalid slots, NOP
        collection[-1] = otherItem;
        collection[short.MaxValue] = otherItem;
    }

    [TestMethod]
    public void TestAdd() {
        var item = CreateItem(1000, amount: 100);
        var collection = new ItemCollection(6);
        IList<(Model.Game.Item Item, int Added)> results = collection.Add(item);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual(item, results[0].Item);
        Assert.AreEqual(100, results[0].Added);
    }

    [TestMethod]
    public void TestAddNoSlotsFree() {
        var item = CreateItem(1000);
        var otherItem = CreateItem(1000);
        var collection = new ItemCollection(1);
        collection.Add(item);

        Assert.AreEqual(0, collection.OpenSlots);
        var results = collection.Add(otherItem);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void TestAddWithoutStacking() {
        var item = CreateItem(1000);
        var stackItem = CreateItem(1000);
        var collection = new ItemCollection(1);
        collection.Add(item);

        // Item can't stack because called with Add(stack=false)
        Assert.AreEqual(0, collection.OpenSlots);
        var results = collection.Add(stackItem);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void TestAddWithStacking() {
        var item = CreateItem(1000, amount: 10);
        var stackItem = CreateItem(1000, amount: 50);
        var collection = new ItemCollection(1);
        collection.Add(item);

        // |stackItem| should be fully stackable.
        Assert.AreEqual(0, collection.GetStackResult(stackItem));

        Assert.AreEqual(0, collection.OpenSlots);
        IList<(Model.Game.Item Item, int Added)> results = collection.Add(stackItem, stack: true);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual(item, results[0].Item); // |stackItem| was stacked onto |item|.
        Assert.AreEqual(50, results[0].Added);

        // |stackItem| should be mutated to have amount=0.
        Assert.AreEqual(0, stackItem.Amount);
    }

    [TestMethod]
    public void TestAddWithStackingMultipleSlots() {
        var item1 = CreateItem(1000, amount: 90);
        var item2 = CreateItem(1000, amount: 95);
        var item3 = CreateItem(1000, amount: 100);
        var stackItem = CreateItem(1000, amount: 50);
        var collection = new ItemCollection(4);
        collection.Add(item1);
        collection.Add(item2);
        collection.Add(item3);

        // |stackItem| should not be fully stackable.
        Assert.AreEqual(35, collection.GetStackResult(stackItem));

        Assert.AreEqual(1, collection.OpenSlots);
        IList<(Model.Game.Item Item, int Added)> results = collection.Add(stackItem, stack: true);
        Assert.AreEqual(3, results.Count);
        Assert.AreEqual(item1, results[0].Item);
        Assert.AreEqual(10, results[0].Added);
        Assert.AreEqual(item2, results[1].Item);
        Assert.AreEqual(5, results[1].Added);
        Assert.AreEqual(stackItem, results[2].Item);
        Assert.AreEqual(35, results[2].Added);
    }

    [TestMethod]
    public void TestAddWithStackingDoesNotFit() {
        var item1 = CreateItem(1000, amount: 90);
        var item2 = CreateItem(1000, amount: 95);
        var item3 = CreateItem(1000, amount: 100);
        var stackItem = CreateItem(1000, amount: 50);
        var collection = new ItemCollection(3);
        collection.Add(item1);
        collection.Add(item2);
        collection.Add(item3);

        Assert.AreEqual(0, collection.OpenSlots);
        var results = collection.Add(stackItem, stack: true);
        Assert.AreEqual(0, results.Count);

        // |collection| and |stackItem| are unchanged.
        Assert.AreEqual(90, collection.Get(item1.Uid)?.Amount);
        Assert.AreEqual(95, collection.Get(item2.Uid)?.Amount);
        Assert.AreEqual(100, collection.Get(item3.Uid)?.Amount);
        Assert.AreEqual(50, stackItem.Amount);
    }

    [TestMethod]
    public void TestRetrieval() {
        var item1 = CreateItem(1000);
        var item2 = CreateItem(2000);
        var collection = new ItemCollection(6) {
            [2] = item1,
            [4] = item2,
        };

        Assert.IsTrue(collection.Contains(item1.Uid));
        Assert.AreEqual(item1, collection.Get(item1.Uid));
        Assert.IsTrue(collection.Contains(item2.Uid));
        Assert.AreEqual(item2, collection.Get(item2.Uid));
        Assert.IsFalse(collection.Contains(0));
        Assert.IsNull(collection.Get(0));
    }

    [TestMethod]
    public void TestRemove() {
        var item = CreateItem(1000);
        var collection = new ItemCollection(6);
        collection.Add(item);
        Assert.AreEqual(collection[0], item);

        // Remove valid item
        Assert.IsTrue(collection.Remove(item.Uid, out Model.Game.Item? removed));
        Assert.AreEqual(item, removed);
        Assert.IsNull(collection[0]);

        // Remove invalid item
        Assert.IsFalse(collection.Remove(-1, out _));
    }

    [TestMethod]
    public void TestRemoveSlot() {
        var item = CreateItem(1000);
        var collection = new ItemCollection(2);
        collection.Add(item);
        Assert.AreEqual(collection[0], item);

        // Remove invalid item
        Assert.IsFalse(collection.RemoveSlot(1, out _));

        // Remove valid item
        Assert.IsTrue(collection.RemoveSlot(0, out Model.Game.Item? removed));
        Assert.AreEqual(item, removed);
        Assert.IsNull(collection[2]);

        // Remove invalid item
        Assert.IsFalse(collection.RemoveSlot(0, out _));
    }

    [TestMethod]
    public void TestSort() {
        var item1 = CreateItem(3000, rarity: 1, amount: 5);
        var item2 = CreateItem(1000, rarity: 3, amount: 10);
        var item3 = CreateItem(1000, rarity: 2, amount: 5);
        var item4 = CreateItem(1000, rarity: 3, amount: 1);
        var item5 = CreateItem(2000, rarity: 4, amount: 20);
        item5.Slot = 6; // Create a gap in inventory

        var collection = new ItemCollection(6);
        collection.Add(item1);
        collection.Add(item2);
        collection.Add(item3);
        collection.Add(item4);
        collection.Add(item5);

        collection.Sort();
        Assert.AreEqual(collection[0], item3);
        Assert.AreEqual(collection[1], item4);
        Assert.AreEqual(collection[2], item2);
        Assert.AreEqual(collection[3], item5);
        Assert.AreEqual(collection[4], item1);
        Assert.IsNull(collection[5]);
    }

    [TestMethod]
    public void TestExpand() {
        var collection = new ItemCollection(1);
        Assert.AreEqual(1, collection.OpenSlots);
        Assert.AreEqual(1, collection.Size);

        Assert.IsTrue(collection.Expand(2));
        Assert.AreEqual(2, collection.OpenSlots);
        Assert.AreEqual(2, collection.Size);
    }

    [TestMethod]
    public void TestExpandNegative() {
        var collection = new ItemCollection(1);
        Assert.IsFalse(collection.Expand(-1));
    }

    [TestMethod]
    public void TestGetStackResult() {
        var item1 = CreateItem(1000, amount: 90);
        var item2 = CreateItem(1000, amount: 95);
        var item3 = CreateItem(1000, amount: 100);
        var collection = new ItemCollection(6);
        collection.Add(item1);
        collection.Add(item2);
        collection.Add(item3);

        var stackItem = CreateItem(1000, amount: 50);
        Assert.AreEqual(35, collection.GetStackResult(stackItem));
        Assert.AreEqual(35, collection.GetStackResult(stackItem, amount: int.MinValue));
        Assert.AreEqual(35, collection.GetStackResult(stackItem, amount: int.MaxValue));
        Assert.AreEqual(0, collection.GetStackResult(stackItem, amount: 0));
        Assert.AreEqual(0, collection.GetStackResult(stackItem, amount: 10));
        Assert.AreEqual(5, collection.GetStackResult(stackItem, amount: 20));
    }

    [TestMethod]
    public void TestEnumeration() {
        var item1 = CreateItem(3000);
        var item2 = CreateItem(1000);
        var item3 = CreateItem(2000);
        var collection = new ItemCollection(10) {
            [1] = item1,
            [3] = item2,
            [5] = item3,
        };

        CollectionAssert.AreEqual(new[] { item1, item2, item3 }, collection.ToList());
    }

    private static Model.Game.Item CreateItem(int id, int rarity = 1, int amount = 1) {
        var fakeProperty = new ItemMetadataProperty(false, 0, 100, 18, 0, string.Empty, ItemTag.None, 0, 0, 0, 0, 0, 0, 0, 0, 0, Array.Empty<int>(), false, 0, false, Array.Empty<int>(), Array.Empty<long>(), Array.Empty<long>(), 0);
        var fakeCustomize = new ItemMetadataCustomize(0, 0);
        var fakeLimit = new ItemMetadataLimit(Gender.All, 0, 0, 4, true, true, true, true, true, false, false, 0, Array.Empty<JobCode>(), Array.Empty<JobCode>());
        var fakeLife = new ItemMetadataLife(0, 0);
        var fakeMetadata = new ItemMetadata(id, $"{id}", Array.Empty<EquipSlot>(), "", Array.Empty<DefaultHairMetadata>(), fakeLife, fakeProperty, fakeCustomize, fakeLimit, null, null, Array.Empty<ItemMetadataAdditionalEffect>(), null, null, null);
        return new Model.Game.Item(fakeMetadata, rarity, amount) { Uid = Rng.NextInt64() };
    }
}
