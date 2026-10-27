using System;
using _Project.Features.Items.Domain;

namespace _Project.Features.Items.Application
{
    public static class StartingInventoryFactory
    {
        public static Inventory Create(IItemCatalog itemCatalog)
        {
            var inventory = new Inventory(itemCatalog);

            if (!inventory.TryAdd(ItemId.IronAxe, 1) ||
                !inventory.TryAdd(ItemId.Wood, 50) ||
                !inventory.TryAdd(ItemId.Apple, 3))
            {
                throw new InvalidOperationException("The starting inventory does not fit in the hotbar.");
            }

            return inventory;
        }
    }
}
