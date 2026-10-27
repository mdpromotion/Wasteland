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

        public ItemIconProvider(ItemCatalogAsset catalog)
        {
            _catalog = catalog;
        }

        public Sprite GetIcon(ItemId itemId)
        {
            return _catalog.Get(itemId).Icon;
        }
    }
}
