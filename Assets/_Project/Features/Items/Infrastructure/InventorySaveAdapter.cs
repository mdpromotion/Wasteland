using System;
using System.Collections.Generic;
using _Project.Features.Items.Application;
using _Project.Features.Items.Domain;
using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    /// <summary>
    /// Maps the domain inventory into <see cref="InventorySnapshot"/>s and back,
    /// tolerating entries referencing items that no longer exist in the catalog.
    /// </summary>
    public sealed class InventorySaveAdapter : IInventoryStatePersistence
    {
        private readonly Inventory _inventory;
        private readonly IItemCatalog _catalog;

        public InventorySaveAdapter(Inventory inventory, IItemCatalog catalog)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public InventorySnapshot Capture()
        {
            var stacks = new List<InventorySlotSnapshot>(Inventory.HotbarSize);
            for (int slot = 0; slot < Inventory.HotbarSize; slot++)
            {
                InventoryStack stack = _inventory.GetSlot(slot);
                if (stack != null)
                    stacks.Add(new InventorySlotSnapshot(slot, stack.Item.Id.Value, stack.Count));
            }

            return new InventorySnapshot(_inventory.SelectedSlot, stacks);
        }

        public int Apply(InventorySnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            _inventory.Clear();
            int skipped = 0;

            foreach (InventorySlotSnapshot entry in snapshot.Stacks)
            {
                if (entry.Slot < 0 || entry.Slot >= Inventory.HotbarSize ||
                    entry.Count <= 0 || string.IsNullOrWhiteSpace(entry.ItemId))
                {
                    skipped++;
                    continue;
                }

                if (!_catalog.TryGet(new ItemId(entry.ItemId), out ItemDefinition definition))
                {
                    skipped++;
                    continue;
                }

                _inventory.SetSlotStack(entry.Slot, definition, entry.Count);
            }

            int selected = snapshot.SelectedSlot;
            if (selected < 0 || selected >= Inventory.HotbarSize)
            {
                selected = 0;
                skipped++;
            }

            _inventory.SelectSlot(selected);
            return skipped;
        }
    }
}
