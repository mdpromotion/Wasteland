using System;
using _Project.Features.Items.Domain;
using NUnit.Framework;

namespace _Project.Tests.Items
{
    public sealed class ItemContainerTests
    {
        private ItemContainer _container;

        [SetUp]
        public void SetUp()
        {
            var catalog = new ItemCatalog(new[]
            {
                new ItemDefinition(ItemId.Wood, "Wood", 50),
                new ItemDefinition(ItemId.Apple, "Apple", 50, new ItemBehavior[] { new ConsumableItemBehavior() })
            });

            _container = new ItemContainer(catalog, 5);
        }

        [Test]
        public void Constructor_RejectsInvalidArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new ItemContainer(null, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ItemContainer(CatalogWithWood(), 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ItemContainer(CatalogWithWood(), -1));
        }

        [Test]
        public void TryAdd_OverCapacity_ReturnsFalseWithoutChanges()
        {
            Assert.That(_container.TryAdd(ItemId.Wood, 1), Is.True);
            Assert.That(_container.FilledSlotCount, Is.EqualTo(1));

            int notifications = 0;
            _container.Changed += () => notifications++;

            // 4 empty slots left with a 50-size stack → 200 fits; asking for 250 fails atomically.
            Assert.That(_container.TryAdd(ItemId.Wood, 250), Is.False);
            Assert.That(_container.GetAt(1), Is.Null);
            Assert.That(notifications, Is.EqualTo(0));

            Assert.That(_container.TryAdd(ItemId.Wood, 200), Is.True);
            Assert.That(_container.FilledSlotCount, Is.EqualTo(5));
        }

        [Test]
        public void TryRemoveAt_EmptySlot_ReturnsFalse()
        {
            Assert.That(_container.TryRemoveAt(0, out ItemDefinition removed), Is.False);
            Assert.That(removed, Is.Null);
        }

        [Test]
        public void TryConsumeAt_NonConsumable_ReturnsFalse()
        {
            _container.TryAdd(ItemId.Wood, 1);

            Assert.That(_container.TryConsumeAt(0), Is.False);
            Assert.That(_container.GetAt(0).Item.Id, Is.EqualTo(ItemId.Wood));
        }

        [Test]
        public void GetAt_OutOfRange_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _container.GetAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _container.GetAt(_container.Capacity));
        }

        private static ItemCatalog CatalogWithWood()
        {
            return new ItemCatalog(new[]
            {
                new ItemDefinition(ItemId.Wood, "Wood", 50)
            });
        }
    }
}
