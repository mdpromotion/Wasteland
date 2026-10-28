using System.Collections.Generic;
using _Project.Features.Items.Application;
using _Project.Features.Persistence.Application;
using _Project.Features.ProceduralWorld.Application.Chunks;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Persistence;
using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    /// <summary>
    /// Respawns persisted dropped items when their owning chunk finishes loading.
    /// Only deltas recorded with <see cref="DeltaAction.Added"/> are spawned; item
    /// removals simply mean "nothing to spawn". Duplicate protection is delegated
    /// to the spawner, so a chunk that reloads while its item entity is still
    /// alive never creates a copy.
    /// </summary>
    public sealed class WorldItemSpawnStage : IWorldItemSpawnStage
    {
        private readonly IChunkDeltaStore _deltaStore;
        private readonly ChunkGrid _chunkGrid;
        private readonly IWorldItemSpawner _spawner;

        public WorldItemSpawnStage(IChunkDeltaStore deltaStore, ChunkGrid chunkGrid, IWorldItemSpawner spawner)
        {
            _deltaStore = deltaStore;
            _chunkGrid = chunkGrid;
            _spawner = spawner;
        }

        public void Spawn(ChunkCoordinate coordinate)
        {
            IReadOnlyList<WorldItemDelta> items = _deltaStore.Load(coordinate).ItemDeltas;
            if (items.Count == 0)
                return;

            Vector2 chunkOrigin = _chunkGrid.ToWorldOffset(coordinate);
            for (int i = 0; i < items.Count; i++)
            {
                WorldItemDelta item = items[i];
                if (item.Action != DeltaAction.Added)
                    continue;

                Vector3 position = new Vector3(
                    chunkOrigin.x + item.X,
                    item.Y,
                    chunkOrigin.y + item.Z);

                _spawner.SpawnPersisted(item.Id, item.ItemKey, position);
            }
        }
    }
}
