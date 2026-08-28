using System.Collections.Generic;
using _Project.Features.ProceduralWorld.Application.Chunks;
using _Project.Features.ProceduralWorld.Application.World;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.World;
using _Project.Features.ProceduralWorld.Infrastructure.Chunks;
using _Project.Features.ProceduralWorld.Presentation.World;
using NUnit.Framework;
using UnityEngine;

namespace _Project.Tests.ProceduralWorld.World
{
    public sealed class WorldRebaseServiceTests
    {
        [Test]
        public void TryRebase_BelowThreshold_DoesNothing()
        {
            var grid = new ChunkGrid(100f, 100f);
            var repository = new FakeChunkLookup();
            var applier = new RecordingRebaseApplier();
            var participant = new RecordingParticipant();

            WorldRebaseSettings settings = CreateSettings(8);

            var service = new WorldRebaseService(
                grid,
                repository,
                settings,
                new[] { participant },
                applier);

            service.TryRebase(new ChunkCoordinate(7, 7));

            Assert.That(applier.MoveCalls, Is.Empty);
            Assert.That(applier.SyncCalls, Is.EqualTo(0));
            Assert.That(participant.Calls, Is.Empty);
            Assert.That(
                grid.OriginCoordinate,
                Is.EqualTo(new ChunkCoordinate(0, 0)));
        }

        [Test]
        public void TryRebase_WhenXReachesThreshold_RebasesWorld()
        {
            var grid = new ChunkGrid(100f, 50f);
            var repository = new FakeChunkLookup();
            var applier = new RecordingRebaseApplier();

            WorldRebaseSettings settings = CreateSettings(8);

            var service = new WorldRebaseService(
                grid,
                repository,
                settings,
                new IWorldRebaseParticipant[0],
                applier);

            service.TryRebase(new ChunkCoordinate(8, 0));

            Assert.That(
                grid.OriginCoordinate,
                Is.EqualTo(new ChunkCoordinate(8, 0)));

            Assert.That(applier.SyncCalls, Is.EqualTo(1));
        }

        [Test]
        public void TryRebase_WhenYReachesThreshold_RebasesWorld()
        {
            var grid = new ChunkGrid(100f, 50f);
            var repository = new FakeChunkLookup();
            var applier = new RecordingRebaseApplier();

            WorldRebaseSettings settings = CreateSettings(8);

            var service = new WorldRebaseService(
                grid,
                repository,
                settings,
                new IWorldRebaseParticipant[0],
                applier);

            service.TryRebase(new ChunkCoordinate(0, 8));

            Assert.That(
                grid.OriginCoordinate,
                Is.EqualTo(new ChunkCoordinate(0, 8)));

            Assert.That(applier.SyncCalls, Is.EqualTo(1));
        }

        [Test]
        public void TryRebase_CalculatesCorrectWorldDelta()
        {
            var grid = new ChunkGrid(100f, 50f);
            var repository = new FakeChunkLookup();
            var applier = new RecordingRebaseApplier();

            repository.Add(CreateChunk(0, 0));

            WorldRebaseSettings settings = CreateSettings(8);

            var service = new WorldRebaseService(
                grid,
                repository,
                settings,
                new IWorldRebaseParticipant[0],
                applier);

            service.TryRebase(new ChunkCoordinate(8, 4));

            Assert.That(
                applier.LastDelta,
                Is.EqualTo(new Vector3(-800f, 0f, -200f)));
        }

        [Test]
        public void TryRebase_MovesEveryChunk()
        {
            var grid = new ChunkGrid(100f, 100f);
            var repository = new FakeChunkLookup();
            var applier = new RecordingRebaseApplier();

            ChunkInstance first = CreateChunk(0, 0);
            ChunkInstance second = CreateChunk(1, 0);
            ChunkInstance third = CreateChunk(0, 1);

            repository.Add(first);
            repository.Add(second);
            repository.Add(third);

            WorldRebaseSettings settings = CreateSettings(8);

            var service = new WorldRebaseService(
                grid,
                repository,
                settings,
                new IWorldRebaseParticipant[0],
                applier);

            service.TryRebase(new ChunkCoordinate(8, 0));

            Assert.That(applier.MoveCalls.Count, Is.EqualTo(3));
            Assert.That(applier.MoveCalls.Exists(x => x.Chunk == first), Is.True);
            Assert.That(applier.MoveCalls.Exists(x => x.Chunk == second), Is.True);
            Assert.That(applier.MoveCalls.Exists(x => x.Chunk == third), Is.True);

            foreach (MoveCall call in applier.MoveCalls)
            {
                Assert.That(
                    call.Delta,
                    Is.EqualTo(new Vector3(-800f, 0f, 0f)));
            }
        }

        [Test]
        public void TryRebase_NotifiesParticipantsInOrder()
        {
            var grid = new ChunkGrid(100f, 100f);
            var repository = new FakeChunkLookup();
            var applier = new RecordingRebaseApplier();

            var calls = new List<int>();

            var first = new RecordingParticipant(10, calls);
            var second = new RecordingParticipant(0, calls);
            var third = new RecordingParticipant(5, calls);

            WorldRebaseSettings settings = CreateSettings(8);

            var service = new WorldRebaseService(
                grid,
                repository,
                settings,
                new[]
                {
                    first,
                    second,
                    third
                },
                applier);

            service.TryRebase(new ChunkCoordinate(8, 0));

            Assert.That(calls, Is.EqualTo(new[] { 0, 5, 10 }));
        }

        [Test]
        public void TryRebase_PassesSameDeltaToParticipants()
        {
            var grid = new ChunkGrid(100f, 50f);
            var repository = new FakeChunkLookup();
            var applier = new RecordingRebaseApplier();

            var first = new RecordingParticipant(0);
            var second = new RecordingParticipant(1);

            WorldRebaseSettings settings = CreateSettings(8);

            var service = new WorldRebaseService(
                grid,
                repository,
                settings,
                new[]
                {
                    first,
                    second
                },
                applier);

            service.TryRebase(new ChunkCoordinate(8, 4));

            Vector3 expectedDelta =
                new Vector3(-800f, 0f, -200f);

            Assert.That(first.LastDelta, Is.EqualTo(expectedDelta));
            Assert.That(second.LastDelta, Is.EqualTo(expectedDelta));
        }

        [Test]
        public void TryRebase_UpdatesGridOriginToNewCenter()
        {
            var grid = new ChunkGrid(100f, 100f);
            var repository = new FakeChunkLookup();
            var applier = new RecordingRebaseApplier();

            WorldRebaseSettings settings = CreateSettings(8);

            var service = new WorldRebaseService(
                grid,
                repository,
                settings,
                new IWorldRebaseParticipant[0],
                applier);

            ChunkCoordinate center =
                new ChunkCoordinate(12, -5);

            service.TryRebase(center);

            Assert.That(
                grid.OriginCoordinate,
                Is.EqualTo(center));
        }

        [Test]
        public void TryRebase_WhenAlreadyAtNewOrigin_DoesNotRebase()
        {
            var grid = new ChunkGrid(100f, 100f);
            grid.SetOriginCoordinate(
                new ChunkCoordinate(10, 10));

            var repository = new FakeChunkLookup();
            var applier = new RecordingRebaseApplier();

            WorldRebaseSettings settings = CreateSettings(8);

            var service = new WorldRebaseService(
                grid,
                repository,
                settings,
                new IWorldRebaseParticipant[0],
                applier);

            service.TryRebase(
                new ChunkCoordinate(10, 10));

            Assert.That(applier.MoveCalls, Is.Empty);
            Assert.That(applier.SyncCalls, Is.EqualTo(0));
        }

        private static WorldRebaseSettings CreateSettings(
            int threshold)
        {
            WorldRebaseSettings settings =
                ScriptableObject.CreateInstance<WorldRebaseSettings>();

            settings.ThresholdChunks = threshold;

            return settings;
        }

        private static ChunkInstance CreateChunk(
            int x,
            int y)
        {
            return new ChunkInstance(
                new ChunkCoordinate(x, y),
                null,
                null,
                null,
                null);
        }

        private sealed class FakeChunkLookup : IChunkLookup
        {
            private readonly List<ChunkInstance> _chunks = new();

            public IEnumerable<ChunkInstance> All => _chunks;

            public bool Contains(ChunkCoordinate coordinate)
            {
                foreach (ChunkInstance chunk in _chunks)
                {
                    if (chunk.Coordinate.Equals(coordinate))
                        return true;
                }

                return false;
            }

            public bool TryGet(
                ChunkCoordinate coordinate,
                out ChunkInstance chunk)
            {
                foreach (ChunkInstance item in _chunks)
                {
                    if (item.Coordinate.Equals(coordinate))
                    {
                        chunk = item;
                        return true;
                    }
                }

                chunk = null;
                return false;
            }

            public ChunkInstance Get(ChunkCoordinate coordinate)
            {
                TryGet(coordinate, out ChunkInstance chunk);
                return chunk;
            }

            public void Add(ChunkInstance chunk)
            {
                _chunks.Add(chunk);
            }
        }

        private sealed class RecordingRebaseApplier :
            IWorldRebaseApplier
        {
            public List<MoveCall> MoveCalls { get; } = new();
            public List<string> Calls { get; } = new();

            public int SyncCalls { get; private set; }

            public Vector3 LastDelta { get; private set; }

            public void MoveChunkTo(
                ChunkInstance chunk,
                Vector3 delta)
            {
                MoveCalls.Add(new MoveCall(chunk, delta));
                LastDelta = delta;
                Calls.Add("Move");
            }

            public void SyncTransforms()
            {
                SyncCalls++;
                Calls.Add("Sync");
            }
        }

        private sealed class MoveCall
        {
            public ChunkInstance Chunk { get; }
            public Vector3 Delta { get; }

            public MoveCall(
                ChunkInstance chunk,
                Vector3 delta)
            {
                Chunk = chunk;
                Delta = delta;
            }
        }

        private sealed class RecordingParticipant :
            IWorldRebaseParticipant
        {
            private readonly List<int> _calls;

            public int Order { get; }

            public Vector3 LastDelta { get; private set; }

            public List<Vector3> Calls { get; } = new();

            public RecordingParticipant(
                int order = 0,
                List<int> calls = null)
            {
                Order = order;
                _calls = calls;
            }

            public void OnWorldRebased(Vector3 delta)
            {
                LastDelta = delta;
                Calls.Add(delta);

                _calls?.Add(Order);
            }
        }
    }
}