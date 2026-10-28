using System;

namespace _Project.Features.Items.Domain
{
    /// <summary>
    /// A fixed-capacity, slot-based storage of item stacks. Knows nothing about selection,
    /// HUD or equipment; the hotbar is a selection window over a container.
    /// </summary>
    public sealed class ItemContainer
    {
        public event Action Changed;

        private readonly IItemCatalog _itemCatalog;
        private readonly InventoryStack[] _slots;

        public int Capacity => _slots.Length;

        /// <summary>Number of slots that currently hold a stack.</summary>
        public int FilledSlotCount
        {
            get
            {
                int filled = 0;
                for (int i = 0; i < _slots.Length; i++)
                    if (_slots[i] != null)
                        filled++;

                return filled;
            }
        }

        public ItemContainer(IItemCatalog itemCatalog, int capacity)
        {
            if (itemCatalog == null)
                throw new ArgumentNullException(nameof(itemCatalog));
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _itemCatalog = itemCatalog;
            _slots = new InventoryStack[capacity];
        }

        public InventoryStack GetAt(int slot)
        {
            ValidateSlot(slot);
            return _slots[slot];
        }

        public bool TryAdd(ItemId itemId, int count)
        {
            if (count <= 0)
                return false;

            ItemDefinition item = _itemCatalog.Get(itemId);

            // Two dry-run passes (stack onto existing stacks, then into empty slots) so
            // that too-small counts never mutate a single slot.
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
                ReplaceAt(i, new InventoryStack(item, stack.Count + added));
                remaining -= added;
            }

            for (int i = 0; i < _slots.Length && remaining > 0; i++)
            {
                if (_slots[i] != null)
                    continue;

                int added = Math.Min(remaining, item.MaxStack);
                ReplaceAt(i, new InventoryStack(item, added));
                remaining -= added;
            }

            Changed?.Invoke();
            return true;
        }

        /// <summary>Removes exactly one item from the slot.</summary>
        public bool TryRemoveAt(int slot, out ItemDefinition removed)
        {
            InventoryStack stack = GetAt(slot);
            if (stack == null)
            {
                removed = null;
                return false;
            }

            removed = stack.Item;
            ReplaceAt(slot, stack.Count == 1
                ? null
                : new InventoryStack(stack.Item, stack.Count - 1));

            Changed?.Invoke();
            return true;
        }

        /// <summary>Consumes one item from the slot if it is consumable.</summary>
        public bool TryConsumeAt(int slot)
        {
            InventoryStack stack = GetAt(slot);
            if (stack == null || stack.Item.GetBehavior<ConsumableItemBehavior>() == null)
                return false;

            return TryRemoveAt(slot, out _);
        }

        public void ReplaceAt(int slot, InventoryStack stack)
        {
            ValidateSlot(slot);
            _slots[slot] = stack;
        }

        private void ValidateSlot(int slot)
        {
            if (slot < 0 || slot >= _slots.Length)
                throw new ArgumentOutOfRangeException(nameof(slot));
        }

        internal void NotifyChanged() => Changed?.Invoke();
    }
}
