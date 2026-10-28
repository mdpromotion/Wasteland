namespace _Project.Features.Persistence.Domain
{
    /// <summary>
    /// Persisted snapshot of a single player inside a single world.
    /// Position is stored as absolute world-space coordinates (origin-independent),
    /// so no chunk-grid origin needs to be persisted alongside it.
    /// <para>
    /// Schema version 2 adds the hotbar inventory. Older files (<c>SchemaVersion</c> 1
    /// or missing) simply have no <see cref="Inventory"/> array, which restores the
    /// starting loadout.
    /// </para>
    /// </summary>
    public sealed class PlayerSaveData
    {
        public int SchemaVersion { get; set; } = 1;

        public string PlayerId { get; set; }

        public double X { get; set; }
        public float Y { get; set; }
        public double Z { get; set; }

        public float Yaw { get; set; }
        public float Pitch { get; set; }

        public long SavedAtTicks { get; set; }

        public InventoryStackSaveData[] Inventory { get; set; }

        public int SelectedSlot { get; set; }
    }

    /// <summary>A single occupied hotbar slot. Slots without an entry are empty.</summary>
    public sealed class InventoryStackSaveData
    {
        public int Slot { get; set; }
        public string ItemId { get; set; }
        public int Count { get; set; }
    }
}
