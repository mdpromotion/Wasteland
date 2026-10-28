using System;
using System.Collections.Generic;

namespace _Project.Features.Items.Domain
{
    public sealed class ItemCatalog : IItemCatalog
    {
        private readonly Dictionary<ItemId, ItemDefinition> _items;

        public ItemCatalog(IEnumerable<ItemDefinition> definitions)
        {
            _items = new Dictionary<ItemId, ItemDefinition>();

            foreach (ItemDefinition definition in definitions)
            {
                if (definition == null)
                    throw new ArgumentException("Item definitions cannot contain null entries.", nameof(definitions));

                if (_items.ContainsKey(definition.Id))
                    throw new ArgumentException($"Duplicate item definition: {definition.Id}.", nameof(definitions));

                _items.Add(definition.Id, definition);
            }
        }

        public ItemDefinition Get(ItemId id)
        {
            if (TryGet(id, out ItemDefinition definition))
                return definition;

            throw new KeyNotFoundException($"No item definition exists for {id}.");
        }

        public bool TryGet(ItemId id, out ItemDefinition definition)
        {
            return _items.TryGetValue(id, out definition);
        }
    }
}
