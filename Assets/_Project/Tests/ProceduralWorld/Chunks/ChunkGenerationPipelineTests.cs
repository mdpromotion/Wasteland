using _Project.Features.ProceduralWorld.Application.Chunks.Generation;
using _Project.Features.ProceduralWorld.Domain;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using NUnit.Framework;
using Unity.Jobs;

namespace _Project.Tests.ProceduralWorld.Chunks
{
    public sealed class ChunkGenerationPipelineTests
    {
        [Test]
        public void Schedule_ExecutesStagesInOrder()
        {
            var pipeline = new ChunkGenerationPipeline();

            var first = new RecordingStage();
            var second = new RecordingStage();
            var third = new RecordingStage();

            pipeline.Add(first);
            pipeline.Add(second);
            pipeline.Add(third);

            ChunkCoordinate coordinate = new ChunkCoordinate(4, 7);

            GenerationTask task = pipeline.Schedule(
                new ChunkGenerationRequest(coordinate, 257));

            Assert.That(first.Calls, Is.EqualTo(1));
            Assert.That(second.Calls, Is.EqualTo(1));
            Assert.That(third.Calls, Is.EqualTo(1));

            Assert.That(first.Coordinate, Is.EqualTo(coordinate));
            Assert.That(second.Coordinate, Is.EqualTo(coordinate));
            Assert.That(third.Coordinate, Is.EqualTo(coordinate));

            Assert.That(first.Resolution, Is.EqualTo(257));
            Assert.That(second.Resolution, Is.EqualTo(257));
            Assert.That(third.Resolution, Is.EqualTo(257));

            Assert.That(task.State.Context.Coordinate, Is.EqualTo(coordinate));
            Assert.That(task.State.Context.Resolution, Is.EqualTo(257));

            task.Handle.Complete();
            task.State.DisposeAll();
        }

        private sealed class RecordingStage : IGenerationStage
        {
            public int Calls { get; private set; }
            public ChunkCoordinate Coordinate { get; private set; }
            public int Resolution { get; private set; }

            public JobHandle Schedule(
                ChunkGenerationState state,
                JobHandle dependency)
            {
                Calls++;

                Coordinate = state.Context.Coordinate;
                Resolution = state.Context.Resolution;

                return dependency;
            }
        }
    }
}