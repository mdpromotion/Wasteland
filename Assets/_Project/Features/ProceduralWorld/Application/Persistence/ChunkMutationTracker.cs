using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Persistence;

namespace _Project.Features.ProceduralWorld.Application.Persistence
{
    /// <summary>
    /// Records in-memory mutations against generated vegetation instances and
    /// dropped world items, marking owning chunks dirty for the save system.
    /// This is the only entry point gameplay code should use when it changes
    /// world state that must persist.
    /// </summary>
    public interface IChunkMutationTracker
    {
        void RecordVegetationRemoved(ChunkCoordinate coord, ulong instanceId);
        void RecordVegetationModified(ChunkCoordinate coord, ulong instanceId, byte[] extraData);

        /// <summary>Records a dropped item appearing in the world (or changing state).</summary>
        void RecordItemAdded(ChunkCoordinate coord, WorldItemDelta itemDelta);

        /// <summary>Records a dropped item being removed from the world (picked up or despawned).</summary>
        void RecordItemRemoved(ChunkCoordinate coord, ulong itemId);

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
        private readonly Dictionary<ChunkCoordinate, Dictionary<ulong, VegetationInstanceDelta>> _pendingVegetation = new();
        private readonly Dictionary<ChunkCoordinate, Dictionary<ulong, WorldItemDelta>> _pendingItems = new();
        private readonly IDirtyChunkRegistry _dirtyRegistry;
        private readonly GeneratorVersionStamp _currentVersions;

        public ChunkMutationTracker(IDirtyChunkRegistry dirtyRegistry, GeneratorVersionStamp currentVersions)
        {
            _dirtyRegistry = dirtyRegistry;
            _currentVersions = currentVersions;
        }

        public void RecordVegetationRemoved(ChunkCoordinate coord, ulong instanceId) =>
            Record(
                _pendingVegetation,
                coord,
                new VegetationInstanceDelta(instanceId, DeltaAction.Removed, null),
                delta => delta.Id);

        public void RecordVegetationModified(ChunkCoordinate coord, ulong instanceId, byte[] extraData) =>
            Record(
                _pendingVegetation,
                coord,
                new VegetationInstanceDelta(instanceId, DeltaAction.Modified, extraData),
                delta => delta.Id);

        public void RecordItemAdded(ChunkCoordinate coord, WorldItemDelta itemDelta)
        {
            if (itemDelta.Action != DeltaAction.Added)
                throw new ArgumentException(
                    $"Expected {nameof(DeltaAction.Added)}, got {itemDelta.Action}. " +
                    "Use RecordItemRemoved for removals.", nameof(itemDelta));

            Record(_pendingItems, coord, itemDelta, delta => delta.Id);
        }

        public void RecordItemRemoved(ChunkCoordinate coord, ulong itemId)
        {
            // Position and item key are irrelevant for removals; the latest entry
            // for an id wins during merge, so only Id and Action matter.
            Record(
                _pendingItems,
                coord,
                new WorldItemDelta(itemId, DeltaAction.Removed, null, 0f, 0f, 0f),
                delta => delta.Id);
        }

        private void Record<TDelta>(
            Dictionary<ChunkCoordinate, Dictionary<ulong, TDelta>> pending,
            ChunkCoordinate coord,
            TDelta delta,
            Func<TDelta, ulong> idSelector)
        {
            if (!pending.TryGetValue(coord, out var map))
            {
                map = new Dictionary<ulong, TDelta>();
                pending[coord] = map;
            }

            map[idSelector(delta)] = delta;
            _dirtyRegistry.MarkDirty(coord);
        }

        public ChunkDelta GetPendingDelta(ChunkCoordinate coord)
        {
            IReadOnlyList<VegetationInstanceDelta> vegetation =
                _pendingVegetation.TryGetValue(coord, out var vegetationMap) && vegetationMap.Count > 0
                    ? vegetationMap.Values.ToArray()
                    : Array.Empty<VegetationInstanceDelta>();

            IReadOnlyList<WorldItemDelta> items =
                _pendingItems.TryGetValue(coord, out var itemsMap) && itemsMap.Count > 0
                    ? itemsMap.Values.ToArray()
                    : Array.Empty<WorldItemDelta>();

            if (vegetation.Count == 0 && items.Count == 0)
                return ChunkDelta.Empty;

            return new ChunkDelta(_currentVersions, vegetation, items);
        }

        public void ClearPending(ChunkCoordinate coord)
        {
            _pendingVegetation.Remove(coord);
            _pendingItems.Remove(coord);
        }
    }
}
