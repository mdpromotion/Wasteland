using System.Collections.Generic;
using _Project.Features.Persistence.Application;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Persistence;
using _Project.Features.ProceduralWorld.Domain.Vegetation;

namespace _Project.Features.ProceduralWorld.Infrastructure.Chunks
{
    public interface IDeltaStage
    {
        void Apply(ChunkCoordinate coordinate, VegetationData vegetation);
    }
    /// <summary>
    /// Applies persisted per-instance deltas (removed/modified) on top of the
    /// deterministically generated baseline vegetation for a chunk.
    /// </summary>
    /// <remarks>
    /// Runs after VegetationGenerator has populated state.Vegetation. Disk access
    /// happens here, so this stage must not be scheduled as a Burst Job — it's
    /// invoked as a synchronous completion step, not through JobHandle chaining.
    /// See note on ChunkManager integration below.
    /// </remarks>
    public sealed class DeltaApplicationStage : IDeltaStage
    {
        private readonly IChunkDeltaStore _store;

        public DeltaApplicationStage(IChunkDeltaStore store) => _store = store;

        public void Apply(ChunkCoordinate coordinate, VegetationData vegetation)
        {
            ChunkDelta delta = _store.Load(coordinate);
            if (delta.IsEmpty)
                return;

            CheckVersion(coordinate, delta.Versions);

            var deltaMap = new Dictionary<ulong, VegetationInstanceDelta>(delta.VegetationDeltas.Count);
            foreach (var d in delta.VegetationDeltas)
                deltaMap[d.Id] = d;

            foreach (var layer in vegetation.Layers)
            {
                var instances = layer.Instances;

                for (int i = instances.Length - 1; i >= 0; i--)
                {
                    if (!deltaMap.TryGetValue(instances[i].Id, out var d))
                        continue;

                    if (d.Action == DeltaAction.Removed)
                        instances.RemoveAtSwapBack(i);
                    // Modified/Added — TODO как раньше
                }
            }
        }

        private void CheckVersion(ChunkCoordinate coord, GeneratorVersionStamp stamp)
        {
            // TODO: if stamp.VegetationVersion does not match the current generator version —
            // decide whether to run the old generator for this chunk's baseline,
            // or log a conflict and proceed anyway (previously deprioritized).
        }
    }
}