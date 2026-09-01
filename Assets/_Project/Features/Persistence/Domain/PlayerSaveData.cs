namespace _Project.Features.Persistence.Domain
{
    /// <summary>
    /// Persisted snapshot of a single player inside a single world.
    /// Position is stored as absolute world-space coordinates (origin-independent),
    /// so no chunk-grid origin needs to be persisted alongside it.
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

        // TODO: inventory. When adding it, bump the SchemaVersion.
    }
}