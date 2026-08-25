using System.Collections.Generic;
using _Project.Features.ProceduralWorld.Domain;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using Unity.Jobs;

namespace _Project.Features.ProceduralWorld.Application.Chunks.Generation
{
    public interface IChunkGenerator
    {
        GenerationTask Schedule(ChunkGenerationRequest request);
    }
    
    /// <summary>
    /// Composes chunk generation stages into a single Job dependency chain.
    /// </summary>
    /// <remarks>
    /// Each stage receives the shared <see cref="ChunkGenerationState"/> and the
    /// <see cref="JobHandle"/> produced by the previous stage. This establishes
    /// execution dependencies without requiring the stages to know about one another.
    /// </remarks>
    public sealed class ChunkGenerationPipeline : IChunkGenerator
    {
        private readonly List<IGenerationStage> _stages = new();

        /// <summary>
        /// Adds a generation stage to the pipeline.
        /// </summary>
        /// <remarks>
        /// Stages execute in the order in which they are added. Later stages receive the
        /// JobHandle produced by earlier stages as their dependency.
        /// </remarks>
        public void Add(IGenerationStage stage)
        {
            _stages.Add(stage);
        }

        /// <summary>
        /// Creates the generation state and schedules all configured stages for a chunk.
        /// </summary>
        /// <returns>
        /// A <see cref="GenerationTask"/> representing the complete scheduled dependency chain.
        /// </returns>
        public GenerationTask Schedule(ChunkGenerationRequest request)
        {
            ChunkGenerationState state = 
                new ChunkGenerationState(
                    new ChunkGenerationContext(
                        request.Coordinate,
                        request.Resolution)
                    );

            JobHandle handle = default;

            foreach (IGenerationStage stage in _stages)
            { 
                handle = stage.Schedule(state, handle);
            }

            return new GenerationTask(handle, state);
        }
    }
}