using System;
using System.Collections.Generic;
using _Project.Features.Items.Application;
using _Project.Features.Persistence.Domain;

namespace _Project.Features.Persistence.Application
{
    /// <summary>
    /// Pure mapping between the persistence DTO (<see cref="PlayerSaveData"/>) and the
    /// Items feature snapshot. Isolated so save compatibility rules are testable.
    /// </summary>
    public static class PlayerInventorySaveMapper
    {
        public const int InventorySchemaVersion = 2;

        /// <summary> Captured inventory into save data. Returns false for an empty snapshot.</summary>
        public static void ApplyToSaveData(InventorySnapshot snapshot, PlayerSaveData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (snapshot == null || snapshot.Stacks.Count == 0)
            {
                data.Inventory = null;
                data.SelectedSlot = 0;
                return;
            }

            var entries = new InventoryStackSaveData[snapshot.Stacks.Count];
            for (int i = 0; i < snapshot.Stacks.Count; i++)
            {
                InventorySlotSnapshot stack = snapshot.Stacks[i];
                entries[i] = new InventoryStackSaveData
                {
                    Slot = stack.Slot,
                    ItemId = stack.ItemId,
                    Count = stack.Count
                };
            }

            data.Inventory = entries;
            data.SelectedSlot = snapshot.SelectedSlot;
            data.SchemaVersion = InventorySchemaVersion;
        }

        /// <summary>
        /// Reads the inventory block from a save. Returns null when the file predates
        /// inventory persistence (schema 1), which restores the starting loadout.
        /// </summary>
        public static InventorySnapshot ToSnapshot(PlayerSaveData data)
        {
            if (data == null || data.Inventory == null || data.Inventory.Length == 0)
                return null;

            var stacks = new List<InventorySlotSnapshot>(data.Inventory.Length);
            for (int i = 0; i < data.Inventory.Length; i++)
            {
                InventoryStackSaveData entry = data.Inventory[i];
                if (entry == null)
                    continue;

                stacks.Add(new InventorySlotSnapshot(entry.Slot, entry.ItemId, entry.Count));
            }

            return new InventorySnapshot(data.SelectedSlot, stacks);
        }
    }
}
