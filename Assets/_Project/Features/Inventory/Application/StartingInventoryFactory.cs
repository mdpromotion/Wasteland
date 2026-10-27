using System;
using System.Collections.Generic;
using _Project.Features.Items.Domain;

namespace _Project.Features.Items.Application
{
    public static class StartingInventoryFactory
    {
        public static Inventory Create(IItemCatalog itemCatalog)
        {
            return Create(itemCatalog, new[]
            {
                new StartingInventoryItem(ItemId.IronAxe, 1),
                new StartingInventoryItem(ItemId.Wood, 50),
                new StartingInventoryItem(ItemId.Apple, 3)
            });
        }

        public static Inventory Create(
            IItemCatalog itemCatalog,
            IEnumerable<StartingInventoryItem> startingItems)
        {
            if (startingItems == null)
                throw new ArgumentNullException(nameof(startingItems));

            var inventory = new Inventory(itemCatalog);
            foreach (StartingInventoryItem startingItem in startingItems)
            {
                if (!inventory.TryAdd(startingItem.ItemId, startingItem.Count))
                    throw new InvalidOperationException(
                        $"The starting inventory does not fit in the hotbar (item '{startingItem.ItemId}').");
            }

            return inventory;
        }
    }

    public readonly struct StartingInventoryItem
    {
        public ItemId ItemId { get; }
        public int Count { get; }

        public StartingInventoryItem(ItemId itemId, int count)
        {
            ItemId = itemId;
            Count = count;
        }
    }
}
