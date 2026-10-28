using System;

namespace _Project.Features.Items.Domain
{
    /// <summary>
    /// Facade over an <see cref="ItemContainer"/> plus the player's <see cref="Hotbar"/>
    /// selection. Keeps the historical API used by gameplay and the HUD while the
    /// storage/selection concerns stay separately extendable (bigger containers, pages).
    /// </summary>
    public sealed class Inventory
    {
        public const int HotbarSize = Hotbar.Size;

        private readonly ItemContainer _container;
        private readonly Hotbar _hotbar;

        public int SelectedSlot => _hotbar.SelectedSlot;
        public ItemDefinition SelectedItem => GetSlot(SelectedSlot)?.Item;

        public event Action Changed;

        public Inventory(IItemCatalog itemCatalog)
            : this(new ItemContainer(itemCatalog, Hotbar.Size), new Hotbar())
        {
        }

        internal Inventory(ItemContainer container, Hotbar hotbar)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _hotbar = hotbar ?? throw new ArgumentNullException(nameof(hotbar));

            _container.Changed += NotifyChanged;
            _hotbar.Changed += NotifyChanged;
        }

        public InventoryStack GetSlot(int slot) => _container.GetAt(slot);

        public bool SelectSlot(int slot) => _hotbar.SelectSlot(slot);

        public bool TryAdd(ItemId itemId, int count) => _container.TryAdd(itemId, count);

        public bool TryConsumeSelected() => _container.TryConsumeAt(_hotbar.SelectedSlot);

        public bool TryRemoveSelectedOne(out ItemDefinition item)
        {
            return _container.TryRemoveAt(_hotbar.SelectedSlot, out item);
        }

        /// <summary>Empties every slot without touching the selected slot.</summary>
        public void Clear()
        {
            for (int i = 0; i < _container.Capacity; i++)
                _container.ReplaceAt(i, null);

            _container.NotifyChanged();
        }

        /// <summary>
        /// Writes a single stack used by save/load restoration. Throws on out-of-range
        /// slots like <see cref="GetSlot"/>, so callers validate their data first.
        /// </summary>
        public void SetSlotStack(int slot, ItemDefinition item, int count)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "A stack count must be positive.");

            _container.ReplaceAt(slot, new InventoryStack(item, count));
            _container.NotifyChanged();
        }

        internal void NotifyChanged() => Changed?.Invoke();
    }
}
