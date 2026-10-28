using System.Collections.Generic;
using _Project.Features.Items.Domain;
using _Project.Features.Items.Infrastructure;
using UnityEngine;

namespace _Project.Features.Items.Presentation
{
    public interface IItemIconProvider
    {
        Sprite GetIcon(ItemId itemId);
    }

    public sealed class ItemIconProvider : IItemIconProvider
    {
        private readonly ItemCatalogAsset _catalog;
        private readonly Dictionary<ItemId, Sprite> _icons = new Dictionary<ItemId, Sprite>();

        public ItemIconProvider(ItemCatalogAsset catalog)
        {
            _catalog = catalog;
        }

        public Sprite GetIcon(ItemId itemId)
        {
            if (_icons.TryGetValue(itemId, out Sprite icon))
                return icon;

            if (!_catalog.TryGet(itemId, out ItemDefinitionAsset definition))
                return null;

            icon = definition.Icon;
            _icons[itemId] = icon;
            return icon;
        }
    }
}
