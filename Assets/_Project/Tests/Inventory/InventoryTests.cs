using System;
using _Project.Features.Items.Application;
using _Project.Features.Items.Domain;
using NUnit.Framework;

namespace _Project.Tests.Items
{
    public sealed class InventoryTests
    {
        private ItemCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _catalog = CreateCatalog();
        }

        [Test]
        public void StartingInventory_ContainsConfiguredItemsAndCounts()
        {
            Inventory inventory = StartingInventoryFactory.Create(_catalog);

            AssertSlot(inventory, 0, ItemId.IronAxe, 1);
            AssertSlot(inventory, 1, ItemId.Wood, 50);
            AssertSlot(inventory, 2, ItemId.Apple, 3);
            Assert.That(inventory.SelectedSlot, Is.EqualTo(0));
        }

        [Test]
        public void TryAdd_SplitsItemsAtMaximumStackSize()
        {
            var inventory = new Inventory(_catalog);

            Assert.That(inventory.TryAdd(ItemId.Wood, 51), Is.True);

            AssertSlot(inventory, 0, ItemId.Wood, 50);
            AssertSlot(inventory, 1, ItemId.Wood, 1);
        }

        [Test]
        public void TryAdd_WhenInventoryIsFull_DoesNotPartiallyAddItems()
        {
            var inventory = new Inventory(_catalog);
            Assert.That(inventory.TryAdd(ItemId.IronAxe, Inventory.HotbarSize), Is.True);

            Assert.That(inventory.TryAdd(ItemId.IronAxe, 1), Is.False);
            for (int slot = 0; slot < Inventory.HotbarSize; slot++)
                AssertSlot(inventory, slot, ItemId.IronAxe, 1);
        }

        [Test]
        public void TryConsumeSelected_ConsumesOneAppleAndClearsLastItem()
        {
            var inventory = new Inventory(_catalog);
            inventory.TryAdd(ItemId.Apple, 2);
            inventory.SelectSlot(0);

            Assert.That(inventory.TryConsumeSelected(), Is.True);
            AssertSlot(inventory, 0, ItemId.Apple, 1);

            Assert.That(inventory.TryConsumeSelected(), Is.True);
            Assert.That(inventory.GetSlot(0), Is.Null);
            Assert.That(inventory.TryConsumeSelected(), Is.False);
        }

        [Test]
        public void TryConsumeSelected_DoesNotConsumeToolsOrMaterials()
        {
            var inventory = new Inventory(_catalog);
            inventory.TryAdd(ItemId.Wood, 5);

            Assert.That(inventory.TryConsumeSelected(), Is.False);
            AssertSlot(inventory, 0, ItemId.Wood, 5);
        }

        [Test]
        public void ItemDefinitions_OnlyIronAxeCanChopAndAppleCanBeConsumed()
        {
            Assert.That(_catalog.Get(ItemId.IronAxe).CanChopTrees, Is.True);
            Assert.That(_catalog.Get(ItemId.IronAxe).ToolDamage, Is.EqualTo(1f));
            Assert.That(_catalog.Get(ItemId.Wood).CanChopTrees, Is.False);
            Assert.That(_catalog.Get(ItemId.Wood).CanConsume, Is.False);
            Assert.That(_catalog.Get(ItemId.Apple).CanConsume, Is.True);
            Assert.That(_catalog.Get(ItemId.Apple).CanChopTrees, Is.False);
        }

        [Test]
        public void ToolDefinition_ExposesCommonAndToolSpecificInspectableFields()
        {
            const float miningPower = 3.5f;
            var tool = new ToolItemDefinition(
                ItemId.IronAxe,
                "Iron Axe",
                1,
                "tool",
                "Tool",
                250f,
                1.5f,
                miningPower,
                new ItemBehavior[] { new TreeChoppingItemBehavior(miningPower) });

            Assert.That(tool.GetInspectableStats().Count, Is.EqualTo(6));
            Assert.That(tool.GetInspectableStats()[3].Label, Is.EqualTo("Durability"));
            Assert.That(tool.GetInspectableStats()[4].Value, Is.EqualTo("1.5"));
            Assert.That(tool.ToolDamage, Is.EqualTo(tool.MiningPower));
        }

        [Test]
        public void FoodAndGenericDefinitions_ExposeOnlyTheirConfiguredFields()
        {
            var food = new FoodItemDefinition(
                ItemId.Apple,
                "Apple",
                50,
                "food",
                "Food",
                4f,
                new ItemBehavior[] { new ConsumableItemBehavior() });
            var wood = new ItemDefinition(
                ItemId.Wood,
                "Wood",
                50,
                "material",
                "Material",
                Array.Empty<ItemBehavior>());

            Assert.That(food.GetInspectableStats().Count, Is.EqualTo(4));
            Assert.That(food.GetInspectableStats()[3].Label, Is.EqualTo("Saturation"));
            Assert.That(food.Saturation, Is.EqualTo(4f));
            Assert.That(wood.GetInspectableStats().Count, Is.EqualTo(3));
        }

        [Test]
        public void SelectSlot_ChangesSelectedItemAndNotifiesObservers()
        {
            var inventory = new Inventory(_catalog);
            inventory.TryAdd(ItemId.IronAxe, 1);
            inventory.TryAdd(ItemId.Wood, 1);
            int notifications = 0;
            inventory.Changed += () => notifications++;

            Assert.That(inventory.SelectSlot(0), Is.False);
            Assert.That(inventory.SelectSlot(1), Is.True);

            Assert.That(inventory.SelectedSlot, Is.EqualTo(1));
            Assert.That(inventory.SelectedItem.Id, Is.EqualTo(ItemId.Wood));
            Assert.That(notifications, Is.EqualTo(1));
        }

        [Test]
        public void SelectSlot_OutOfRange_Throws()
        {
            var inventory = new Inventory(_catalog);

            Assert.Throws<ArgumentOutOfRangeException>(() => inventory.SelectSlot(Inventory.HotbarSize));
        }

        private static void AssertSlot(Inventory inventory, int slot, ItemId itemId, int count)
        {
            InventoryStack stack = inventory.GetSlot(slot);
            Assert.That(stack, Is.Not.Null);
            Assert.That(stack.Item.Id, Is.EqualTo(itemId));
            Assert.That(stack.Count, Is.EqualTo(count));
        }

        private static ItemCatalog CreateCatalog()
        {
            return new ItemCatalog(new[]
            {
                new ItemDefinition(ItemId.IronAxe, "Iron Axe", 1, canChopTrees: true, toolDamage: 1f),
                new ItemDefinition(ItemId.Wood, "Wood", 50),
                new ItemDefinition(ItemId.Apple, "Apple", 50, canConsume: true)
            });
        }
    }
}