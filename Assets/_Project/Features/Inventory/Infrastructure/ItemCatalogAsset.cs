using System;
using _Project.Features.Items.Domain;
using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    [CreateAssetMenu(menuName = "Project/Items/Item Catalog", fileName = "ItemCatalog")]
    public sealed class ItemCatalogAsset : ScriptableObject
    {
        [SerializeField] private ItemDefinitionAsset[] items = Array.Empty<ItemDefinitionAsset>();

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
