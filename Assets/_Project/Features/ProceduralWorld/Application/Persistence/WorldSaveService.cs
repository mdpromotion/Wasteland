using System;
using System.Linq;
using _Project.Features.Persistence.Application;
using _Project.Features.ProceduralWorld.Domain.Chunks;

namespace _Project.Features.ProceduralWorld.Application.Persistence
{
    /// <summary>
    /// Single contract for persisting chunk deltas. Manual save, autosave, and
    /// unload-triggered flush must all go through this — no other code should
    /// call ChunkDeltaStore directly.
    /// </summary>
    public interface IWorldSaveService
    {
        /// <summary>Flushes a single chunk's pending delta to disk, if dirty.</summary>
        void SaveChunk(ChunkCoordinate coord);

        /// <summary>Flushes every currently dirty chunk. Used by autosave and manual "save game".</summary>
        void SaveAllDirty();

        event Action<ChunkCoordinate> ChunkSaved;
        event Action WorldSaved;
    }

    public sealed class WorldSaveService : IWorldSaveService
    {
        private readonly IChunkMutationTracker _tracker;
        private readonly IDirtyChunkRegistry _dirtyRegistry;
        private readonly ChunkDeltaStore _deltaStore;

        public event Action<ChunkCoordinate> ChunkSaved;
        public event Action WorldSaved;

        public WorldSaveService(
            IChunkMutationTracker tracker,
            IDirtyChunkRegistry dirtyRegistry,
            ChunkDeltaStore deltaStore)
        {
            _tracker = tracker;
            _dirtyRegistry = dirtyRegistry;
            _deltaStore = deltaStore;
        }

        public void SaveChunk(ChunkCoordinate coord)
        {
            var delta = _tracker.GetPendingDelta(coord);
            if (delta.IsEmpty)
            {
                _dirtyRegistry.MarkClean(coord);
                return;
            }

            _deltaStore.Save(coord, delta);
            _tracker.ClearPending(coord);
            _dirtyRegistry.MarkClean(coord);
            ChunkSaved?.Invoke(coord);
        }

        public void SaveAllDirty()
        {
            foreach (var coord in _dirtyRegistry.DirtyChunks.ToArray())
                SaveChunk(coord);

            WorldSaved?.Invoke();
        }
    }
}