using System;
using _Project.Features.Items.Application;
using _Project.Features.Items.Domain;
using _Project.Features.Items.Infrastructure;
using _Project.Features.Persistence.Application;
using _Project.Features.Persistence.Domain;
using _Project.Features.Persistence.Infrastructure;
using NUnit.Framework;
using UnityEngine;

namespace _Project.Tests.Items
{
    public sealed class InventorySaveAdapterTests
    {
        private ItemCatalog _catalog;
        private Inventory _inventory;
        private InventorySaveAdapter _adapter;

        [SetUp]
        public void SetUp()
        {
            _catalog = Catalog();
            _inventory = new Inventory(_catalog);
            _adapter = new InventorySaveAdapter(_inventory, _catalog);
        }

        [Test]
        public void Capture_MapsStacksAndSelection()
        {
            _inventory.TryAdd(ItemId.IronAxe, 1);
            _inventory.TryAdd(ItemId.Wood, 50);
            _inventory.SelectSlot(1);

            InventorySnapshot snapshot = _adapter.Capture();

            Assert.That(snapshot.SelectedSlot, Is.EqualTo(1));
            Assert.That(snapshot.Stacks.Count, Is.EqualTo(2));
            AssertSlot(snapshot, 0, "iron_axe", 1);
            AssertSlot(snapshot, 1, "wood", 50);
        }

        [Test]
        public void Capture_EmptyInventory_GivesNoStacks()
        {
            InventorySnapshot snapshot = _adapter.Capture();

            Assert.That(snapshot.Stacks.Count, Is.EqualTo(0));
            Assert.That(snapshot.SelectedSlot, Is.EqualTo(0));
        }

        [Test]
        public void Capture_ThenApply_RestoresSameState()
        {
            _inventory.TryAdd(ItemId.IronAxe, 1);
            _inventory.TryAdd(ItemId.Wood, 37);
            _inventory.TryAdd(ItemId.Apple, 3);
            _inventory.SelectSlot(2);

            InventorySnapshot captured = _adapter.Capture();

            var restoredInventory = new Inventory(_catalog);
            var restoredAdapter = new InventorySaveAdapter(restoredInventory, _catalog);
            restoredAdapter.Apply(captured);

            for (int slot = 0; slot < Inventory.HotbarSize; slot++)
            {
                InventoryStack expected = _inventory.GetSlot(slot);
                InventoryStack actual = restoredInventory.GetSlot(slot);
                if (expected == null)
                    Assert.That(actual, Is.Null);
                else
                {
                    Assert.That(actual.Item.Id, Is.EqualTo(expected.Item.Id));
                    Assert.That(actual.Count, Is.EqualTo(expected.Count));
                }
            }

            Assert.That(restoredInventory.SelectedSlot, Is.EqualTo(2));
        }

        [Test]
        public void Apply_SkipsInvalidAndUnknownEntries()
        {
            _inventory.TryAdd(ItemId.Wood, 1); // to be cleared by Apply

            var snapshot = new InventorySnapshot(0, new[]
            {
                new InventorySlotSnapshot(0, "iron_axe", 1),   // valid
                new InventorySlotSnapshot(1, "ghost_item", 5), // unknown id
                new InventorySlotSnapshot(2, "wood", 0),       // invalid count
                new InventorySlotSnapshot(-1, "wood", 4),      // bad slot
                new InventorySlotSnapshot(Inventory.HotbarSize, "wood", 4), // bad slot
                new InventorySlotSnapshot(3, "", 4),           // empty id
                new InventorySlotSnapshot(4, null, 4)          // null id
            });

            int skipped = _adapter.Apply(snapshot);

            Assert.That(skipped, Is.EqualTo(6));
            Assert.That(_inventory.GetSlot(0).Item.Id, Is.EqualTo(ItemId.IronAxe));
            for (int slot = 1; slot < Inventory.HotbarSize; slot++)
                Assert.That(_inventory.GetSlot(slot), Is.Null);
        }

        [Test]
        public void Apply_ReplacesExistingContent()
        {
            _inventory.TryAdd(ItemId.Wood, 50);
            _inventory.TryAdd(ItemId.Apple, 3);

            var snapshot = new InventorySnapshot(0, new[]
            {
                new InventorySlotSnapshot(5, "apple", 1)
            });

            _adapter.Apply(snapshot);

            Assert.That(_inventory.GetSlot(0), Is.Null);
            Assert.That(_inventory.GetSlot(1), Is.Null);
            Assert.That(_inventory.GetSlot(5).Item.Id, Is.EqualTo(ItemId.Apple));
            Assert.That(_inventory.GetSlot(5).Count, Is.EqualTo(1));
        }

        [Test]
        public void Apply_NullSnapshot_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _adapter.Apply(null));
        }

        private static void AssertSlot(InventorySnapshot snapshot, int slot, string itemId, int count)
        {
            foreach (InventorySlotSnapshot stack in snapshot.Stacks)
                if (stack.Slot == slot)
                {
                    Assert.That(stack.ItemId, Is.EqualTo(itemId));
                    Assert.That(stack.Count, Is.EqualTo(count));
                    return;
                }

            Assert.Fail($"Snapshot has no stack in slot {slot}.");
        }

        private static ItemCatalog Catalog()
        {
            return new ItemCatalog(new[]
            {
                new ItemDefinition(
                    ItemId.IronAxe,
                    "Iron Axe",
                    1,
                    new ItemBehavior[] { new TreeChoppingItemBehavior(1f) }),
                new ItemDefinition(ItemId.Wood, "Wood", 50),
                new ItemDefinition(ItemId.Apple, "Apple", 50, new ItemBehavior[] { new ConsumableItemBehavior() })
            });
        }
    }
}
