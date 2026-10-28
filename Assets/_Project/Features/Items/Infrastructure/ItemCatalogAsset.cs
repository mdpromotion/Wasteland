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

        private Dictionary<ItemId, ItemDefinitionAsset> _index;

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
            if (TryGet(id, out ItemDefinitionAsset definition))
                return definition;

            throw new InvalidOperationException($"Item catalog '{name}' has no item with ID '{id}'.");
        }

        public bool TryGet(ItemId id, out ItemDefinitionAsset definition)
        {
            if (_index == null)
                BuildIndex();

            return _index.TryGetValue(id, out definition);
        }

        private void BuildIndex()
        {
            _index = new Dictionary<ItemId, ItemDefinitionAsset>();
            ItemDefinitionAsset[] configuredItems = items ?? Array.Empty<ItemDefinitionAsset>();

            // Last entry wins; duplicates are kept rather than throwing so that a
            // mis-edited asset stays runnable in game, only flagged in the editor.
            for (int i = 0; i < configuredItems.Length; i++)
            {
                if (configuredItems[i] == null)
                    continue;

                _index[configuredItems[i].Id] = configuredItems[i];
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _index = null;
        }
#endif
    }
}
