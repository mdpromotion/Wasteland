using System;
using System.Collections.Generic;
using _Project.Features.Items.Application;
using _Project.Features.Items.Domain;
using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    [CreateAssetMenu(menuName = "Project/Items/Starting Inventory", fileName = "StartingInventory")]
    public sealed class StartingInventoryAsset : ScriptableObject
    {
        [Serializable]
        private sealed class Entry
        {
            public ItemDefinitionAsset item;
            [Min(1)] public int count = 1;
        }

        [SerializeField] private Entry[] items = Array.Empty<Entry>();

        public Inventory Create(IItemCatalog catalog)
        {
            Entry[] configuredItems = items ?? Array.Empty<Entry>();
            var startingItems = new List<StartingInventoryItem>(configuredItems.Length);
            for (int i = 0; i < configuredItems.Length; i++)
            {
                Entry entry = configuredItems[i];
                if (entry == null || entry.item == null)
                    throw new InvalidOperationException($"Starting inventory '{name}' has a missing item at index {i}.");
                if (entry.count <= 0)
                    throw new InvalidOperationException($"Starting inventory '{name}' has an invalid count at index {i}.");

                startingItems.Add(new StartingInventoryItem(entry.item.Id, entry.count));
            }

            return StartingInventoryFactory.Create(catalog, startingItems);
        }
    }
}
