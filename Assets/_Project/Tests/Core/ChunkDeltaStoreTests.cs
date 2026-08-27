using System;
using System.Collections.Generic;
using System.IO;
using _Project.Features.Core.Persistence.Regions;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Persistence;
using NUnit.Framework;

namespace _Project.Tests.Core.Persistence
{
    public class ChunkDeltaStoreTests
    {
        private PalRegionFileStore _fileStore;
        private ChunkDeltaStore _deltaStore;
        private string _regionsDir;

        [SetUp]
        public void SetUp()
        {
            _fileStore = new PalRegionFileStore();
            _deltaStore = new ChunkDeltaStore(_fileStore, _fileStore, new ChunkDeltaSerializer());
            _regionsDir = Path.Combine(UnityEngine.Application.persistentDataPath, "Regions");

            if (Directory.Exists(_regionsDir))
                Directory.Delete(_regionsDir, recursive: true);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_regionsDir))
                Directory.Delete(_regionsDir, recursive: true);
        }

        [Test]
        public void Load_OnUntouchedChunk_ReturnsEmpty_AndDoesNotCreateFile()
        {
            var delta = _deltaStore.Load(new ChunkCoordinate(3, 3));

            Assert.IsTrue(delta.IsEmpty);
            Assert.IsFalse(Directory.Exists(_regionsDir));
        }

        [Test]
        public void SaveThenLoad_RoundTripsDelta()
        {
            var coord = new ChunkCoordinate(5, -2);
            var original = new ChunkDelta(
                new GeneratorVersionStamp(1),
                new List<VegetationInstanceDelta> { new VegetationInstanceDelta(99UL, DeltaAction.Removed, null) });

            _deltaStore.Save(coord, original);
            var loaded = _deltaStore.Load(coord);

            Assert.IsFalse(loaded.IsEmpty);
            Assert.AreEqual(99UL, loaded.VegetationDeltas[0].Id);
            Assert.AreEqual(DeltaAction.Removed, loaded.VegetationDeltas[0].Action);
        }

        [Test]
        public void SaveThenLoad_PreservesGeneratorVersion()
        {
            var coord = new ChunkCoordinate(0, 0);
            var original = new ChunkDelta(
                new GeneratorVersionStamp(7),
                new List<VegetationInstanceDelta> { new VegetationInstanceDelta(1UL, DeltaAction.Removed, null) });

            _deltaStore.Save(coord, original);
            var loaded = _deltaStore.Load(coord);

            Assert.AreEqual(7, loaded.Versions.VegetationVersion);
        }

        [Test]
        public void Save_EmptyDelta_DeletesSlotInsteadOfWriting()
        {
            var coord = new ChunkCoordinate(1, 1);

            _deltaStore.Save(coord, new ChunkDelta(default,
                new List<VegetationInstanceDelta> { new VegetationInstanceDelta(1UL, DeltaAction.Removed, null) }));
            _deltaStore.Save(coord, ChunkDelta.Empty);

            Assert.IsTrue(_deltaStore.Load(coord).IsEmpty);
        }

        [Test]
        public void Save_EmptyDelta_OnNeverWrittenChunk_DoesNotCreateFile()
        {
            _deltaStore.Save(new ChunkCoordinate(2, 2), ChunkDelta.Empty);

            Assert.IsFalse(Directory.Exists(_regionsDir));
        }

        [Test]
        public void Save_TwoChunksInSameRegion_DoNotOverwriteEachOther()
        {
            var coordA = new ChunkCoordinate(0, 0);
            var coordB = new ChunkCoordinate(1, 0);

            _deltaStore.Save(coordA, new ChunkDelta(default,
                new List<VegetationInstanceDelta> { new VegetationInstanceDelta(1UL, DeltaAction.Removed, null) }));
            _deltaStore.Save(coordB, new ChunkDelta(default,
                new List<VegetationInstanceDelta> { new VegetationInstanceDelta(2UL, DeltaAction.Removed, null) }));

            Assert.AreEqual(1UL, _deltaStore.Load(coordA).VegetationDeltas[0].Id);
            Assert.AreEqual(2UL, _deltaStore.Load(coordB).VegetationDeltas[0].Id);
        }

        [Test]
        public void Save_ChunksInDifferentRegions_UseDifferentFiles()
        {
            var coordA = new ChunkCoordinate(0, 0);
            var coordB = new ChunkCoordinate(16, 0); // соседний регион по X

            _deltaStore.Save(coordA, new ChunkDelta(default,
                new List<VegetationInstanceDelta> { new VegetationInstanceDelta(1UL, DeltaAction.Removed, null) }));
            _deltaStore.Save(coordB, new ChunkDelta(default,
                new List<VegetationInstanceDelta> { new VegetationInstanceDelta(2UL, DeltaAction.Removed, null) }));

            Assert.IsTrue(File.Exists(Path.Combine(_regionsDir, "r.0.0.pal")));
            Assert.IsTrue(File.Exists(Path.Combine(_regionsDir, "r.1.0.pal")));
        }

        [Test]
        public void Overwrite_SameChunk_LatestDeltaWins()
        {
            var coord = new ChunkCoordinate(4, 4);

            _deltaStore.Save(coord, new ChunkDelta(default,
                new List<VegetationInstanceDelta> { new VegetationInstanceDelta(1UL, DeltaAction.Removed, null) }));
            _deltaStore.Save(coord, new ChunkDelta(default,
                new List<VegetationInstanceDelta> { new VegetationInstanceDelta(2UL, DeltaAction.Removed, null) }));

            var loaded = _deltaStore.Load(coord);
            Assert.AreEqual(1, loaded.VegetationDeltas.Count);
            Assert.AreEqual(2UL, loaded.VegetationDeltas[0].Id);
        }

        [Test]
        public void Load_TombstonedSlot_ReturnsEmptyNotThrow()
        {
            var coord = new ChunkCoordinate(0, 0);
            _deltaStore.Save(coord, new ChunkDelta(default,
                new List<VegetationInstanceDelta> { new VegetationInstanceDelta(1UL, DeltaAction.Removed, null) }));

            var (regionX, regionZ, slot) = ((int, int, int))typeof(ChunkDeltaStore)
                .Assembly.GetType("_Project.Features.Core.Persistence.Regions.RegionAddressing")
                .GetMethod("ToSlot")!
                .Invoke(null, new object[] { coord });

            _fileStore.DeleteSlot(regionX, regionZ, slot);

            Assert.DoesNotThrow(() => _deltaStore.Load(coord));
            Assert.IsTrue(_deltaStore.Load(coord).IsEmpty);
        }
    }
}