using System;

namespace _Project.Features.Items.Domain
{
    public sealed class Inventory
    {
        public const int HotbarSize = 9;

        private readonly IItemCatalog _itemCatalog;
        private readonly InventoryStack[] _slots = new InventoryStack[HotbarSize];

        public int SelectedSlot { get; private set; }
        public ItemDefinition SelectedItem => _slots[SelectedSlot]?.Item;

        public event Action Changed;

        public Inventory(IItemCatalog itemCatalog)
        {
            _itemCatalog = itemCatalog ?? throw new ArgumentNullException(nameof(itemCatalog));
        }

        public InventoryStack GetSlot(int slot)
        {
            ValidateSlot(slot);
            return _slots[slot];
        }

        public bool SelectSlot(int slot)
        {
            ValidateSlot(slot);
            if (SelectedSlot == slot)
                return false;

            SelectedSlot = slot;
            Changed?.Invoke();
            return true;
        }

        public bool TryAdd(ItemId itemId, int count)
        {
            if (count <= 0)
                return false;

            ItemDefinition item = _itemCatalog.Get(itemId);
            int remaining = count;

            for (int i = 0; i < _slots.Length && remaining > 0; i++)
            {
                InventoryStack stack = _slots[i];
                if (stack == null || stack.Item.Id != itemId)
                    continue;

                remaining -= Math.Min(remaining, item.MaxStack - stack.Count);
            }

            for (int i = 0; i < _slots.Length && remaining > 0; i++)
            {
                if (_slots[i] != null)
                    continue;

                remaining -= Math.Min(remaining, item.MaxStack);
            }

            if (remaining > 0)
                return false;

            remaining = count;
            for (int i = 0; i < _slots.Length && remaining > 0; i++)
            {
                InventoryStack stack = _slots[i];
                if (stack == null || stack.Item.Id != itemId)
                    continue;

                int added = Math.Min(remaining, item.MaxStack - stack.Count);
                _slots[i] = new InventoryStack(item, stack.Count + added);
                remaining -= added;
            }

            for (int i = 0; i < _slots.Length && remaining > 0; i++)
            {
                if (_slots[i] != null)
                    continue;

                int added = Math.Min(remaining, item.MaxStack);
                _slots[i] = new InventoryStack(item, added);
                remaining -= added;
            }

            Changed?.Invoke();
            return true;
        }

        public bool TryConsumeSelected()
        {
            InventoryStack stack = _slots[SelectedSlot];
            if (stack == null || !stack.Item.CanConsume)
                return false;

            _slots[SelectedSlot] = stack.Count == 1
                ? null
                : new InventoryStack(stack.Item, stack.Count - 1);

            Changed?.Invoke();
            return true;
        }

        private static void ValidateSlot(int slot)
        {
            if (slot < 0 || slot >= HotbarSize)
                throw new ArgumentOutOfRangeException(nameof(slot));
        }
    }
}
