using System.Collections.Generic;

namespace _Project.Features.Items.Application
{
    /// <summary>
    /// Feature-owned bridge used by the persistence feature to save and restore
    /// inventory state without knowing the concrete inventory implementation.
    /// </summary>
    public interface IInventoryStatePersistence
    {
        /// <summary>Current slots and hotbar selection as a serializable snapshot.</summary>
        InventorySnapshot Capture();

        /// <summary>
        /// Replaces the whole inventory with the snapshot's content.
        /// Returns the number of skipped (invalid or unknown) entries.
        /// </summary>
        int Apply(InventorySnapshot snapshot);
    }

    public sealed class InventorySnapshot
    {
        public static readonly InventorySnapshot Empty =
            new InventorySnapshot(0, new InventorySlotSnapshot[0]);

        public int SelectedSlot { get; }
        public IReadOnlyList<InventorySlotSnapshot> Stacks { get; }

        public InventorySnapshot(int selectedSlot, IReadOnlyList<InventorySlotSnapshot> stacks)
        {
            SelectedSlot = selectedSlot;
            Stacks = stacks ?? new InventorySlotSnapshot[0];
        }
    }

    public readonly struct InventorySlotSnapshot
    {
        public int Slot { get; }
        public string ItemId { get; }
        public int Count { get; }

        public InventorySlotSnapshot(int slot, string itemId, int count)
        {
            Slot = slot;
            ItemId = itemId;
            Count = count;
        }
    }
}
