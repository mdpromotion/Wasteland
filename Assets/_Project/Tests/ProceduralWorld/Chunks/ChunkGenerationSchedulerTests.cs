using System.Collections.Generic;
using NUnit.Framework;
using _Project.Features.Core.Infrastructure;
using _Project.Features.ProceduralWorld.Application.Chunks.Generation;
using _Project.Features.ProceduralWorld.Domain;
using _Project.Features.ProceduralWorld.Domain.Chunks;

namespace _Project.Tests.ProceduralWorld.Chunks
{
    public sealed class ChunkGenerationSchedulerTests
    {
        [Test]
        public void Tick_SchedulesQueuedRequest_AndCompletesIt()
        {
            var generator = new FakeGenerator();
            var budget = new FakeFrameBudget();

            var scheduler =
                new ChunkGenerationScheduler(generator, budget);

            var coordinate = new ChunkCoordinate(2, 3);

            scheduler.Enqueue(
                new ChunkGenerationRequest(coordinate, 257));

            var applied = new List<ChunkCoordinate>();
            var completed = new List<ChunkCoordinate>();

            scheduler.Tick(
                result =>
                {
                    applied.Add(result.State.Context.Coordinate);
                    result.Dispose();
                },
                completed.Add);

            Assert.That(generator.Requests.Count, Is.EqualTo(1));
            Assert.That(
                generator.Requests[0].Coordinate,
                Is.EqualTo(coordinate));

            Assert.That(applied, Is.EqualTo(new[] { coordinate }));
            Assert.That(completed, Is.EqualTo(new[] { coordinate }));
        }

        [Test]
        public void Enqueue_SameCoordinateTwice_SchedulesOnlyOnce()
        {
            var generator = new FakeGenerator();
            var scheduler =
                new ChunkGenerationScheduler(
                    generator,
                    new FakeFrameBudget());

            var coordinate = new ChunkCoordinate(1, 1);

            scheduler.Enqueue(
                new ChunkGenerationRequest(coordinate, 257));

            scheduler.Enqueue(
                new ChunkGenerationRequest(coordinate, 512));

            scheduler.Tick(
                result => result.Dispose(),
                _ => { });

            Assert.That(generator.Requests.Count, Is.EqualTo(1));
            Assert.That(
                generator.Requests[0].Resolution,
                Is.EqualTo(257));
        }

        [Test]
        public void Cancel_QueuedRequest_PreventsScheduling()
        {
            var generator = new FakeGenerator();

            var scheduler =
                new ChunkGenerationScheduler(
                    generator,
                    new FakeFrameBudget());

            var coordinate = new ChunkCoordinate(5, 5);

            scheduler.Enqueue(
                new ChunkGenerationRequest(coordinate, 257));

            scheduler.Cancel(coordinate);

            scheduler.Tick(
                result => result.Dispose(),
                _ => { });

            Assert.That(generator.Requests, Is.Empty);
        }

        [Test]
        public void Cancel_RunningRequest_CompletesAsCancelled()
        {
            var generator = new FakeGenerator();
            var scheduler =
                new ChunkGenerationScheduler(
                    generator,
                    new FakeFrameBudget());

            var coordinate = new ChunkCoordinate(5, 5);

            scheduler.Enqueue(
                new ChunkGenerationRequest(coordinate, 257));
            
            scheduler.Tick(
                result => result.Dispose(),
                _ => { });

            scheduler.Enqueue(
                new ChunkGenerationRequest(
                    new ChunkCoordinate(6, 6),
                    257));
        }

        [Test]
        public void Tick_DoesNotCompleteMoreOperationsThanFrameBudgetAllows()
        {
            var generator = new FakeGenerator();
            var budget = new FakeFrameBudget
            {
                AllowOperations = 1
            };

            var scheduler =
                new ChunkGenerationScheduler(generator, budget);

            var first = new ChunkCoordinate(0, 0);
            var second = new ChunkCoordinate(1, 0);
            var third = new ChunkCoordinate(2, 0);

            scheduler.Enqueue(
                new ChunkGenerationRequest(first, 257));

            scheduler.Enqueue(
                new ChunkGenerationRequest(second, 257));

            scheduler.Enqueue(
                new ChunkGenerationRequest(third, 257));

            var completed = new List<ChunkCoordinate>();

            scheduler.Tick(
                result => result.Dispose(),
                completed.Add);

            Assert.That(completed.Count, Is.EqualTo(1));
            Assert.That(completed[0], Is.EqualTo(first));
        }

        [Test]
        public void Tick_SchedulesAtMostTenJobs()
        {
            var generator = new DeferredFakeGenerator();
            var scheduler =
                new ChunkGenerationScheduler(
                    generator,
                    new FakeFrameBudget
                    {
                        AllowOperations = 0
                    });

            for (int i = 0; i < 15; i++)
            {
                scheduler.Enqueue(
                    new ChunkGenerationRequest(
                        new ChunkCoordinate(i, 0),
                        257));
            }

            scheduler.Tick(
                result => result.Dispose(),
                _ => { });

            Assert.That(generator.Requests.Count, Is.EqualTo(10));
        }

        [Test]
        public void CompleteAll_ClearsQueuedAndRunningTasks()
        {
            var generator = new DeferredFakeGenerator();
            var scheduler =
                new ChunkGenerationScheduler(
                    generator,
                    new FakeFrameBudget
                    {
                        AllowOperations = 0
                    });

            for (int i = 0; i < 5; i++)
            {
                scheduler.Enqueue(
                    new ChunkGenerationRequest(
                        new ChunkCoordinate(i, 0),
                        257));
            }

            scheduler.Tick(
                result => result.Dispose(),
                _ => { });

            Assert.That(generator.Requests.Count, Is.EqualTo(5));

            scheduler.CompleteAll();
            
            scheduler.Tick(
                result => result.Dispose(),
                _ => { });

            Assert.That(generator.Requests.Count, Is.EqualTo(5));
        }

        private class FakeGenerator : IChunkGenerator
        {
            public List<ChunkGenerationRequest> Requests { get; } = new();

            public virtual GenerationTask Schedule(
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

        private sealed class DeferredFakeGenerator : FakeGenerator
        {
            public override GenerationTask Schedule(
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

        private sealed class FakeFrameBudget : IFrameBudget
        {
            public int AllowOperations { get; set; } = int.MaxValue;

            public bool TryBeginOperation(
                out IFrameBudgetOperation operation)
            {
                if (AllowOperations <= 0)
                {
                    operation = null;
                    return false;
                }

                AllowOperations--;

                operation = new FakeFrameBudgetOperation();

                return true;
            }
        }

        private sealed class FakeFrameBudgetOperation :
            IFrameBudgetOperation
        {
            public void Dispose()
            {
            }
        }
    }
}