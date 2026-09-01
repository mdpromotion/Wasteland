using System.Collections.Generic;
using System.IO;
using System.Linq;
using _Project.Features.Persistence.Application;
using _Project.Features.Persistence.Domain;
using _Project.Features.Persistence.Infrastructure;
using _Project.Features.ProceduralWorld.Application.Persistence;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Persistence;
using _Project.Features.ProceduralWorld.Domain.World;
using NUnit.Framework;

namespace _Project.Tests.ProceduralWorld.Persistence
{
    public class WorldSaveServiceTests
    {
        private class FakeMutationTracker : IChunkMutationTracker
        {
            public Dictionary<ChunkCoordinate, ChunkDelta> Deltas = new();
            public List<ChunkCoordinate> Cleared = new();

            public void RecordVegetationRemoved(ChunkCoordinate coord, ulong id) { }
            public void RecordVegetationModified(ChunkCoordinate coord, ulong id, byte[] extra) { }

            public ChunkDelta GetPendingDelta(ChunkCoordinate coord) =>
                Deltas.TryGetValue(coord, out var d) ? d : ChunkDelta.Empty;

            public void ClearPending(ChunkCoordinate coord) => Cleared.Add(coord);
        }

        private PalRegionFileStore _fileStore;
        private FakeWorldSettings _worldSettings;
        private ChunkDeltaStore _deltaStore;
        private FakeMutationTracker _tracker;
        private DirtyChunkRegistry _dirtyRegistry;
        private WorldSaveService _saveService;
        private string _regionsDir;

        [SetUp]
        public void SetUp()
        {
            _worldSettings = new FakeWorldSettings { Name = "Test-World" };
            _fileStore = new PalRegionFileStore(_worldSettings);
            _deltaStore = new ChunkDeltaStore(_fileStore, _fileStore, new ChunkDeltaSerializer());
            _regionsDir = Path.Combine(
                UnityEngine.Application.persistentDataPath,
                "Worlds",
                _worldSettings.Name,
                "Regions");

            if (Directory.Exists(_regionsDir))
                Directory.Delete(_regionsDir, recursive: true);

            _tracker = new FakeMutationTracker();
            _dirtyRegistry = new DirtyChunkRegistry();
            _saveService = new WorldSaveService(_tracker, _dirtyRegistry, _deltaStore);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_regionsDir))
                Directory.Delete(_regionsDir, recursive: true);
        }

        private static ChunkDelta OneRemoval(ulong id) =>
            new ChunkDelta(default, new List<VegetationInstanceDelta> { new VegetationInstanceDelta(id, DeltaAction.Removed, null) });

        [Test]
        public void SaveChunk_WithPendingDelta_WritesToDeltaStoreAndClearsTracker()
        {
            var coord = new ChunkCoordinate(0, 0);
            _tracker.Deltas[coord] = OneRemoval(1UL);
            _dirtyRegistry.MarkDirty(coord);

            _saveService.SaveChunk(coord);

            Assert.IsFalse(_deltaStore.Load(coord).IsEmpty);
            CollectionAssert.Contains(_tracker.Cleared, coord);
            Assert.IsFalse(_dirtyRegistry.DirtyChunks.Contains(coord));
        }

        [Test]
        public void SaveChunk_WithoutPendingDelta_MarksCleanWithoutWritingFile()
        {
            var coord = new ChunkCoordinate(1, 1);
            _dirtyRegistry.MarkDirty(coord);

            _saveService.SaveChunk(coord);

            Assert.IsTrue(_deltaStore.Load(coord).IsEmpty);
            Assert.IsFalse(_dirtyRegistry.DirtyChunks.Contains(coord));
            Assert.IsFalse(Directory.Exists(_regionsDir));
        }

        [Test]
        public void SaveChunk_RaisesChunkSavedEvent()
        {
            var coord = new ChunkCoordinate(0, 0);
            _tracker.Deltas[coord] = OneRemoval(1UL);

            ChunkCoordinate? raised = null;
            _saveService.ChunkSaved += c => raised = c;

            _saveService.SaveChunk(coord);

            Assert.AreEqual(coord, raised);
        }

        [Test]
        public void SaveChunk_UntrackedAndNotDirty_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _saveService.SaveChunk(new ChunkCoordinate(99, 99)));
        }

        [Test]
        public void SaveAllDirty_FlushesEveryDirtyChunk()
        {
            var a = new ChunkCoordinate(0, 0);
            var b = new ChunkCoordinate(1, 0);
            _tracker.Deltas[a] = OneRemoval(1UL);
            _tracker.Deltas[b] = OneRemoval(2UL);
            _dirtyRegistry.MarkDirty(a);
            _dirtyRegistry.MarkDirty(b);

            _saveService.SaveAllDirty();

            Assert.IsFalse(_dirtyRegistry.HasDirtyChunks);
            Assert.IsFalse(_deltaStore.Load(a).IsEmpty);
            Assert.IsFalse(_deltaStore.Load(b).IsEmpty);
        }

        [Test]
        public void SaveAllDirty_OnEmptyRegistry_DoesNotThrowAndDoesNotRaiseWorldSaved()
        {
            bool raised = false;
            _saveService.WorldSaved += () => raised = true;
            
            Assert.DoesNotThrow(() => _saveService.SaveAllDirty());
            Assert.IsFalse(raised);

        }


        [Test]
        public void SaveAllDirty_DirtyButNoPendingDelta_StillMarksClean()
        {
            var coord = new ChunkCoordinate(2, 2);
            _dirtyRegistry.MarkDirty(coord);

            _saveService.SaveAllDirty();

            Assert.IsFalse(_dirtyRegistry.HasDirtyChunks);
        }
        internal sealed class FakeWorldSettings : IWorldSettings
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
