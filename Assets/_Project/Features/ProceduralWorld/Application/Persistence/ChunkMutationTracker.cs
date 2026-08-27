using System.Collections.Generic;
using System.Linq;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Persistence;

namespace _Project.Features.ProceduralWorld.Application.Persistence
{
    /// <summary>
    /// Records in-memory mutations against generated vegetation instances and
    /// marks the owning chunk dirty for the save system. This is the only entry
    /// point gameplay code should use when it changes world state that must persist.
    /// </summary>
    public interface IChunkMutationTracker
    {
        void RecordVegetationRemoved(ChunkCoordinate coord, ulong instanceId);
        void RecordVegetationModified(ChunkCoordinate coord, ulong instanceId, byte[] extraData);

        /// <summary>
        /// Returns the currently buffered delta for a chunk (empty if untouched since last save).
        /// </summary>
        ChunkDelta GetPendingDelta(ChunkCoordinate coord);

        /// <summary>
        /// Clears the buffer for a chunk after it has been successfully persisted.
        /// </summary>
        void ClearPending(ChunkCoordinate coord);
    }

    public sealed class ChunkMutationTracker : IChunkMutationTracker
    {
        private readonly Dictionary<ChunkCoordinate, Dictionary<ulong, VegetationInstanceDelta>> _pending = new();
        private readonly IDirtyChunkRegistry _dirtyRegistry;
        private readonly GeneratorVersionStamp _currentVersions;

        public ChunkMutationTracker(IDirtyChunkRegistry dirtyRegistry, GeneratorVersionStamp currentVersions)
        {
            _dirtyRegistry = dirtyRegistry;
            _currentVersions = currentVersions;
        }

        public void RecordVegetationRemoved(ChunkCoordinate coord, ulong instanceId) =>
            Record(coord, new VegetationInstanceDelta(instanceId, DeltaAction.Removed, null));

        public void RecordVegetationModified(ChunkCoordinate coord, ulong instanceId, byte[] extraData) =>
            Record(coord, new VegetationInstanceDelta(instanceId, DeltaAction.Modified, extraData));

        private void Record(ChunkCoordinate coord, VegetationInstanceDelta delta)
        {
            if (!_pending.TryGetValue(coord, out var map))
            {
                map = new Dictionary<ulong, VegetationInstanceDelta>();
                _pending[coord] = map;
            }

            map[delta.Id] = delta;
            _dirtyRegistry.MarkDirty(coord);
        }

        public ChunkDelta GetPendingDelta(ChunkCoordinate coord)
        {
            if (!_pending.TryGetValue(coord, out var map) || map.Count == 0)
                return ChunkDelta.Empty;

            return new ChunkDelta(_currentVersions, map.Values.ToArray());
        }

        public void ClearPending(ChunkCoordinate coord) => _pending.Remove(coord);
    }
}