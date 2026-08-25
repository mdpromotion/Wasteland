namespace _Project.Features.Core.Persistence.Regions
{
    public enum PalSlotState
    {
        Missing,
        
        Tombstoned,
        
        Present,
        
        Corrupted
    }

    public readonly struct PalSlotReadResult
    {
        public readonly PalSlotState State;
        public readonly byte[] Payload;

        public PalSlotReadResult(PalSlotState state, byte[] payload)
        {
            State = state;
            Payload = payload;
        }

        public static readonly PalSlotReadResult Missing =
            new PalSlotReadResult(PalSlotState.Missing, null);

        public static readonly PalSlotReadResult Tombstoned =
            new PalSlotReadResult(PalSlotState.Tombstoned, null);
    }

    public interface IPalRegionWriter
    {
        /// Writes (or overwrites) the payload for a chunk slot inside a
        /// region file, addressed by region coordinates + a flat slot index
        /// (0..255, i.e. localZ * 16 + localX).
        ///
        /// A zero-length payload is valid and distinct from "never written":
        /// it represents a chunk that was touched but currently has no delta.
        ///
        /// Append-only: previously written bytes for this slot are left
        /// orphaned in the file. Reclaiming that space is a compaction
        /// concern, not this writer's job.
        void WriteSlot(int regionX, int regionZ, int slotIndex, byte[] payload);

        /// Marks a slot as deleted. The slot's old payload bytes (if any)
        /// stay orphaned in the data area; only the index entry changes.
        void DeleteSlot(int regionX, int regionZ, int slotIndex);
    }

    public interface IPalRegionReader
    {
        PalSlotReadResult ReadSlot(int regionX, int regionZ, int slotIndex);
    }
}