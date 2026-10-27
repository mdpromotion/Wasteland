using System.Collections.Generic;
using _Project.Features.Items.Domain;
using UnityEngine;

namespace _Project.Features.Items.Presentation
{
    public interface IItemIconProvider
    {
        Sprite GetIcon(ItemId itemId);
    }

    public sealed class ItemIconProvider : IItemIconProvider
    {
        private readonly IReadOnlyDictionary<ItemId, Sprite> _icons;

        public ItemIconProvider(Sprite ironAxe, Sprite wood, Sprite apple)
        {
            _icons = new Dictionary<ItemId, Sprite>
            {
                [ItemId.IronAxe] = ironAxe,
                [ItemId.Wood] = wood,
                [ItemId.Apple] = apple
            };
        }

        public Sprite GetIcon(ItemId itemId)
        {
            return _icons.TryGetValue(itemId, out Sprite icon) ? icon : null;
        }
    }
}
