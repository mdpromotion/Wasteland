using System;
using System.Collections.Generic;
using _Project.Features.Core.Infrastructure;
using _Project.Features.ProceduralWorld.Application.Chunks;
using _Project.Features.ProceduralWorld.Application.Chunks.Generation;
using _Project.Features.ProceduralWorld.Application.Persistence;
using _Project.Features.ProceduralWorld.Domain;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Vegetation;
using _Project.Features.ProceduralWorld.Infrastructure.Chunks;
using _Project.Features.ProceduralWorld.Infrastructure.Interfaces;
using _Project.Features.ProceduralWorld.Presentation.Landscape;
using NUnit.Framework;
using UnityEngine;

namespace _Project.Tests.ProceduralWorld.Chunks
{
    public sealed class ChunkManagerTests
    {
        [Test]
        public void QueueLoad_SameCoordinateTwice_GeneratesOnlyOnce()
        {
            var generator = new RecordingGenerator();

            var scheduler =
                new ChunkGenerationScheduler(
                    generator,
                    new UnlimitedFrameBudget());

            var repository = new ChunkRepository();

            var applier = new RecordingApplier();

            var manager =
                new ChunkManager(
                    scheduler,
                    repository,
                    applier,
                    new FakeLandscapeFactory(),
                    new ChunkNeighborConnector(
                        new FakeLandscapeFactory()),
                    new FakeDeltaStage(),
                    new FakeWorldSaveService());

            ChunkCoordinate coordinate =
                new ChunkCoordinate(5, 10);

            manager.QueueLoad(coordinate);
            manager.QueueLoad(coordinate);

            manager.Tick();

            Assert.That(generator.Requests.Count, Is.EqualTo(1));
            Assert.That(
                generator.Requests[0].Coordinate,
                Is.EqualTo(coordinate));
        }

        [Test]
        public void QueueLoad_AlreadyLoadedChunk_DoesNotGenerate()
        {
            var generator = new RecordingGenerator();

            var scheduler =
                new ChunkGenerationScheduler(
                    generator,
                    new UnlimitedFrameBudget());

            var repository = new ChunkRepository();

            ChunkCoordinate coordinate =
                new ChunkCoordinate(5, 10);

            repository.Add(
                new ChunkInstance(
                    coordinate,
                    null,
                    null,
                    null));

            var manager =
                new ChunkManager(
                    scheduler,
                    repository,
                    new RecordingApplier(),
                    new FakeLandscapeFactory(),
                    new ChunkNeighborConnector(
                        new FakeLandscapeFactory()),
                    new FakeDeltaStage(),
                    new FakeWorldSaveService());

            manager.QueueLoad(coordinate);
            manager.Tick();

            Assert.That(generator.Requests, Is.Empty);
        }
        
        private sealed class SpyDeltaStage : IDeltaStage
        {
            public readonly List<ChunkCoordinate> AppliedFor = new();

            public void Apply(ChunkCoordinate coordinate, VegetationData vegetation) =>
                AppliedFor.Add(coordinate);
        }

        private sealed class SpyWorldSaveService : IWorldSaveService
        {
            public readonly List<ChunkCoordinate> SavedChunks = new();

            public event Action<ChunkCoordinate> ChunkSaved;
            public event Action WorldSaved;

            public void SaveChunk(ChunkCoordinate coord) => SavedChunks.Add(coord);
            public void SaveAllDirty() { }
        }

        [Test]
        public void Unload_ChunkNotInRepository_DoesNotCallSaveChunk()
        {
            var repository = new ChunkRepository();
            var saveService = new SpyWorldSaveService();

            var manager = new ChunkManager(
                new ChunkGenerationScheduler(new RecordingGenerator(), new UnlimitedFrameBudget()),
                repository,
                new RecordingApplier(),
                new FakeLandscapeFactory(),
                new ChunkNeighborConnector(new FakeLandscapeFactory()),
                new FakeDeltaStage(),
                saveService);

            manager.Unload(new ChunkCoordinate(50, 50));

            Assert.IsEmpty(saveService.SavedChunks);
        }

        private sealed class RecordingGenerator : IChunkGenerator
        {
            public List<ChunkGenerationRequest> Requests { get; } = new();

            public GenerationTask Schedule(
                ChunkGenerationRequest request)
            {
                Requests.Add(request);

                var state =
                    new ChunkGenerationState(
                        new ChunkGenerationContext(
                            request.Coordinate,
                            request.Resolution));

                return new GenerationTask(
                    default,
                    state);
            }
        }

        private sealed class UnlimitedFrameBudget :
            IFrameBudget
        {
            public bool TryBeginOperation(
                out IFrameBudgetOperation operation)
            {
                operation = new Operation();
                return true;
            }

            private sealed class Operation :
                IFrameBudgetOperation
            {
                public void Dispose()
                {
                }
            }
        }

        private sealed class RecordingApplier :
            ILandscapeApplier
        {
            public readonly List<ChunkGenerationResult> Results = new();

            public void Apply(ChunkGenerationResult result)
            {
                Results.Add(result);
            }
        }

        private sealed class FakeDeltaStage : IDeltaStage
        {
            public void Apply(
                ChunkCoordinate coordinate,
                _Project.Features.ProceduralWorld.Domain.Vegetation.VegetationData vegetation)
            {
            }
        }

        private sealed class FakeWorldSaveService : IWorldSaveService
        {
            public event Action<ChunkCoordinate> ChunkSaved;
            public event Action WorldSaved;

            public void SaveChunk(ChunkCoordinate coord)
            {
            }

            public void SaveAllDirty()
            {
            }
        }

        private sealed class FakeLandscapeFactory :
            ILandscapeFactory
        {
            public Terrain Create(ChunkCoordinate coordinate, Transform parent)
            {
                throw new NotImplementedException();
            }

            public void Connect(
                Terrain self,
                Terrain left,
                Terrain top,
                Terrain right,
                Terrain bottom)
            {
            }

            public void Show(Terrain terrain)
            {
            }

            public void Release(Terrain terrain)
            {
            }
        }
    }
}