using _Project.Features.ProceduralWorld.Application.Persistence;
using _Project.Features.ProceduralWorld.Domain.Chunks;

namespace _Project.Features.Items.Infrastructure
{
    /// <summary>
    /// Binds one dropped-item entity to its persistence record: the owning chunk,
    /// its stable id and the chunk mutation tracker.
    /// </summary>
    /// <remarks>
    /// A Removed delta is written only when the item explicitly leaves the world
    /// (collected or rolled back), because scene teardown destroys objects the
    /// same way as removal — and a destroyed scene object is not evidence that
    /// the item left the world.
    /// </remarks>
    public sealed class DroppedItemHandle
    {
        private readonly IChunkMutationTracker _mutationTracker;
        private readonly WorldItemRegistry _registry;
        private readonly ChunkCoordinate _chunk;

        public ulong Id { get; }

        private bool _removedFromWorld;

        public DroppedItemHandle(
            IChunkMutationTracker mutationTracker,
            WorldItemRegistry registry,
            ChunkCoordinate chunk,
            ulong id)
        {
            _mutationTracker = mutationTracker;
            _registry = registry;
            _chunk = chunk;
            Id = id;
        }

        /// <summary>Records the item leaving the world: registry cleanup + Removed delta.</summary>
        public void ReleaseFromWorld(WorldItemPickup source)
        {
            if (_removedFromWorld)
                return;

            _removedFromWorld = true;
            _registry.Release(Id, source);
            _mutationTracker.RecordItemRemoved(_chunk, Id);
        }

        /// <summary>Frees the registry slot only (scene teardown, quit, reload).</summary>
        public void ReleaseFromScene(WorldItemPickup source)
        {
            _registry.Release(Id, source);
        }
    }
}
