using _Project.Features.ProceduralWorld.Domain.Chunks;

namespace _Project.Features.ProceduralWorld.Application.Chunks
{
    /// <summary>
    /// Spawns persisted dropped items (item deltas recorded with
    /// <see cref="DeltaAction.Added"/>) for a chunk that finishes loading.
    /// </summary>
    /// <remarks>
    /// The contract lives in the ProceduralWorld application layer so the chunk
    /// pipeline stays agnostic of the item feature; the concrete implementation
    /// is provided by the item feature's infrastructure.
    /// </remarks>
    public interface IWorldItemSpawnStage
    {
        void Spawn(ChunkCoordinate coordinate);
    }
}
