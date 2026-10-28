using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Features.Persistence.Application;
using _Project.Features.Persistence.Domain;
using _Project.Features.Persistence.Infrastructure;
using _Project.Features.ProceduralWorld.Application.Persistence;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Persistence;
using _Project.Features.ProceduralWorld.Domain.World;
using NUnit.Framework;

namespace _Project.Tests.Core.Persistence
{
    public class ChunkMutationTrackerItemTests
    {
        private DirtyChunkRegistry _dirtyRegistry;
        private ChunkMutationTracker _tracker;

        [SetUp]
        public void SetUp()
        {
            _dirtyRegistry = new DirtyChunkRegistry();
            _tracker = new ChunkMutationTracker(_dirtyRegistry, new GeneratorVersionStamp(1));
        }

        [Test]
        public void RecordItemAdded_BufferedWithAllFields_AndMarksChunkDirty()
        {
            var coord = new ChunkCoordinate(2, 3);
            var delta = new WorldItemDelta(42UL, DeltaAction.Added, "wood", 1f, 2f, 3f);

            _tracker.RecordItemAdded(coord, delta);

            WorldItemDelta pending = _tracker.GetPendingDelta(coord).ItemDeltas[0];
            Assert.AreEqual(42UL, pending.Id);
            Assert.AreEqual(DeltaAction.Added, pending.Action);
            Assert.AreEqual("wood", pending.ItemKey);
            Assert.AreEqual(1f, pending.X);
            Assert.AreEqual(2f, pending.Y);
            Assert.AreEqual(3f, pending.Z);
            Assert.IsTrue(_dirtyRegistry.DirtyChunks.Contains(coord));
        }

        [Test]
        public void RecordItemRemoved_AfterRecordItemAdded_SameIdWins()
        {
            var coord = new ChunkCoordinate(2, 3);
            _tracker.RecordItemAdded(coord, new WorldItemDelta(42UL, DeltaAction.Added, "wood", 1f, 2f, 3f));

            _tracker.RecordItemRemoved(coord, 42UL);

            var items = _tracker.GetPendingDelta(coord).ItemDeltas;
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(DeltaAction.Removed, items[0].Action);
        }

        [Test]
        public void RecordItemAdded_WithRemovedAction_Throws()
        {
            var coord = new ChunkCoordinate(0, 0);
            var invalid = new WorldItemDelta(1UL, DeltaAction.Removed, "wood", 0f, 0f, 0f);

            Assert.Throws<System.ArgumentException>(
                () => _tracker.RecordItemAdded(coord, invalid));
        }

        [Test]
        public void GetPendingDelta_ItemAndVegetationInSameChunk_Combined()
        {
            var coord = new ChunkCoordinate(4, 4);
            _tracker.RecordVegetationRemoved(coord, 7UL);
            _tracker.RecordItemAdded(coord, new WorldItemDelta(8UL, DeltaAction.Added, "apple", 1f, 2f, 3f));

            ChunkDelta pending = _tracker.GetPendingDelta(coord);

            Assert.AreEqual(1, pending.VegetationDeltas.Count);
            Assert.AreEqual(1, pending.ItemDeltas.Count);
            Assert.AreEqual(7UL, pending.VegetationDeltas[0].Id);
            Assert.AreEqual(8UL, pending.ItemDeltas[0].Id);
        }

        [Test]
        public void GetPendingDelta_UntouchedChunk_ReturnsEmpty()
        {
            var pending = _tracker.GetPendingDelta(new ChunkCoordinate(9, 9));

            Assert.IsTrue(pending.IsEmpty);
        }

        [Test]
        public void ClearPending_DropsItemRecords()
        {
            var coord = new ChunkCoordinate(2, 3);
            _tracker.RecordItemAdded(coord, new WorldItemDelta(1UL, DeltaAction.Added, "wood", 1f, 1f, 1f));

            _tracker.ClearPending(coord);

            Assert.IsTrue(_tracker.GetPendingDelta(coord).IsEmpty);
        }

        [Test]
        public void SaveChunk_FlushesItemDeltaToStore_AndClearsPending()
        {
            var worldSettings = new FakeWorldSettings { Name = "Tracker-Item-Test" };
            var fileStore = new PalRegionFileStore(worldSettings);
            var store = new ChunkDeltaStore(
                fileStore, fileStore, new ChunkDeltaSerializer());
            var saveService = new WorldSaveService(_tracker, _dirtyRegistry, store);

            var coord = new ChunkCoordinate(6, 6);
            _tracker.RecordItemAdded(coord, new WorldItemDelta(5UL, DeltaAction.Added, "apple", 2f, 3f, 4f));

            saveService.SaveChunk(coord);

            WorldItemDelta saved = store.Load(coord).ItemDeltas[0];
            Assert.AreEqual(5UL, saved.Id);
            Assert.AreEqual("apple", saved.ItemKey);
            Assert.IsTrue(_tracker.GetPendingDelta(coord).IsEmpty);
            Assert.IsFalse(_dirtyRegistry.DirtyChunks.Contains(coord));

            string regionsDir = System.IO.Path.Combine(
                UnityEngine.Application.persistentDataPath, "Worlds", worldSettings.Name, "Regions");
            if (System.IO.Directory.Exists(regionsDir))
                System.IO.Directory.Delete(regionsDir, true);
        }

        private sealed class FakeWorldSettings : IWorldSettings
        {
            public string Name { get; set; } = "Test-World";
            public int Seed { get; set; }
            public int Octaves { get; set; }
            public float Scale { get; set; }
            public float Persistence { get; set; }
            public float Lacunarity { get; set; }
            public float RedistributionPower { get; set; }
        }
    }
}
