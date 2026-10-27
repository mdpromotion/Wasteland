using System;
using System.Collections.Generic;
using _Project.Features.Items.Application;
using _Project.Features.Items.Domain;
using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    [CreateAssetMenu(menuName = "Project/Items/Item Catalog", fileName = "ItemCatalog")]
    public sealed class ItemCatalogAsset : ScriptableObject
    {
        [Serializable]
        private sealed class StartingItemEntry
        {
            [SerializeField] private ItemDefinitionAsset item;
            [SerializeField, Min(1)] private int count = 1;

            public ItemDefinitionAsset Item => item;
            public int Count => count;
        }

        [SerializeField] private ItemDefinitionAsset[] items = Array.Empty<ItemDefinitionAsset>();
        [SerializeField] private StartingItemEntry[] startingItems = Array.Empty<StartingItemEntry>();

        public ItemCatalog CreateDomainCatalog()
        {
            ItemDefinitionAsset[] configuredItems = items ?? Array.Empty<ItemDefinitionAsset>();
            var definitions = new ItemDefinition[configuredItems.Length];
            for (int i = 0; i < configuredItems.Length; i++)
            {
                if (configuredItems[i] == null)
                    throw new InvalidOperationException($"Item catalog '{name}' has a missing item at index {i}.");

                definitions[i] = configuredItems[i].CreateDomainDefinition();
            }

            return new ItemCatalog(definitions);
        }

        public Inventory CreateStartingInventory(IItemCatalog catalog)
        {
            StartingItemEntry[] configuredItems = startingItems ?? Array.Empty<StartingItemEntry>();
            var entries = new List<StartingInventoryItem>(configuredItems.Length);
            for (int i = 0; i < configuredItems.Length; i++)
            {
                StartingItemEntry entry = configuredItems[i];
                if (entry == null || entry.Item == null)
                    throw new InvalidOperationException($"Item catalog '{name}' has a missing starting item at index {i}.");
                if (entry.Count <= 0)
                    throw new InvalidOperationException($"Item catalog '{name}' has an invalid starting count at index {i}.");

                entries.Add(new StartingInventoryItem(entry.Item.Id, entry.Count));
            }

            return StartingInventoryFactory.Create(catalog, entries);
        }

        public ItemDefinitionAsset Get(ItemId id)
        {
            ItemDefinitionAsset[] configuredItems = items ?? Array.Empty<ItemDefinitionAsset>();
            for (int i = 0; i < configuredItems.Length; i++)
                if (configuredItems[i] != null && configuredItems[i].Id == id)
                    return configuredItems[i];

            throw new InvalidOperationException($"Item catalog '{name}' has no item with ID '{id}'.");
        }
    }
}
