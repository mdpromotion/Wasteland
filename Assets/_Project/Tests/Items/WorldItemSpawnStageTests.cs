using System.Collections.Generic;
using _Project.Features.Items.Domain;
using _Project.Features.Items.Infrastructure;
using _Project.Features.Persistence.Application;
using _Project.Features.ProceduralWorld.Application.Chunks;
using _Project.Features.Items.Application;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Persistence;
using NUnit.Framework;
using UnityEngine;

namespace _Project.Tests.Items
{
    public class WorldItemSpawnStageTests
    {
        private FakeChunkDeltaStore _store;
        private FakeSpawner _spawner;
        private WorldItemSpawnStage _stage;

        [SetUp]
        public void SetUp()
        {
            _store = new FakeChunkDeltaStore();
            _spawner = new FakeSpawner();
            // Chunk sizes chosen asymmetrically to catch X/Z mix-ups (100 x 50).
            _stage = new WorldItemSpawnStage(_store, new ChunkGrid(100f, 50f), _spawner);
        }

        [Test]
        public void Spawn_AddedItemDelta_RespawnsAtChunkLocalPosition()
        {
            var coord = new ChunkCoordinate(2, -1);
            _store.Deltas[coord] = new ChunkDelta(
                default,
                new List<VegetationInstanceDelta>(),
                new List<WorldItemDelta>
                {
                    new WorldItemDelta(7UL, DeltaAction.Added, "wood", 10f, 2f, 20f),
                });

            _stage.Spawn(coord);

            Assert.AreEqual(1, _spawner.Requests.Count);
            Assert.AreEqual(7UL, _spawner.Requests[0].EntityId);
            Assert.AreEqual("wood", _spawner.Requests[0].ItemKey);
            // Chunk origin in Unity space is (200, -50); item coordinates are chunk-local.
            Assert.AreEqual(new Vector3(210f, 2f, -30f), _spawner.Requests[0].Position);
        }

        [Test]
        public void Spawn_RemovedItemOnly_DoesNotSpawn()
        {
            var coord = new ChunkCoordinate(0, 0);
            _store.Deltas[coord] = new ChunkDelta(
                default,
                new List<VegetationInstanceDelta>(),
                new List<WorldItemDelta>
                {
                    new WorldItemDelta(7UL, DeltaAction.Removed, null, 0f, 0f, 0f),
                });

            _stage.Spawn(coord);

            Assert.AreEqual(0, _spawner.Requests.Count);
        }

        [Test]
        public void Spawn_NoItemDeltas_DoesNotSpawn()
        {
            var coord = new ChunkCoordinate(1, 1);
            _store.Deltas[coord] = ChunkDelta.Empty;

            _stage.Spawn(coord);

            Assert.AreEqual(0, _spawner.Requests.Count);
        }

        [Test]
        public void Spawn_MultipleAddedItems_EachGetsItsOwnRequest()
        {
            var coord = new ChunkCoordinate(-3, 2);
            _store.Deltas[coord] = new ChunkDelta(
                default,
                new List<VegetationInstanceDelta>(),
                new List<WorldItemDelta>
                {
                    new WorldItemDelta(1UL, DeltaAction.Added, "wood", 0f, 0f, 0f),
                    new WorldItemDelta(2UL, DeltaAction.Added, "iron_axe", 1f, 1f, 1f),
                });

            _stage.Spawn(coord);

            Assert.AreEqual(2, _spawner.Requests.Count);
            Assert.AreEqual("wood", _spawner.Requests[0].ItemKey);
            Assert.AreEqual("iron_axe", _spawner.Requests[1].ItemKey);
        }

        [Test]
        public void Spawn_UntouchedChunk_LoadsAndSkips()
        {
            _stage.Spawn(new ChunkCoordinate(9, 9));

            Assert.AreEqual(0, _spawner.Requests.Count);
        }

        private sealed class FakeChunkDeltaStore : IChunkDeltaStore
        {
            public readonly Dictionary<ChunkCoordinate, ChunkDelta> Deltas = new();

            public ChunkDelta Load(ChunkCoordinate coord) =>
                Deltas.TryGetValue(coord, out var delta) ? delta : ChunkDelta.Empty;

            public void Save(ChunkCoordinate coord, ChunkDelta incrementalDelta) { }

            public void Unload(ChunkCoordinate coord) { }
        }

        private sealed class SpawnRequest
        {
            public ulong EntityId;
            public string ItemKey;
            public Vector3 Position;
        }

        private sealed class FakeSpawner : IWorldItemSpawner
        {
            public readonly List<SpawnRequest> Requests = new();

            public WorldItemPickup SpawnFresh(
                ItemId itemId, Vector3 position, Quaternion rotation, Vector3 velocity) =>
                throw new System.NotSupportedException("SpawnStage must never build fresh drops.");

            public WorldItemPickup SpawnPersisted(ulong entityId, string itemKey, Vector3 position)
            {
                Requests.Add(new SpawnRequest
                {
                    EntityId = entityId,
                    ItemKey = itemKey,
                    Position = position,
                });
                return null;
            }
        }
    }
}
