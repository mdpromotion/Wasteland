using _Project.Features.ProceduralWorld.Domain.Chunks;
using Unity.Jobs;

namespace _Project.Features.ProceduralWorld.Application.Chunks.Generation
{
    /// <summary>
    /// Represents one stage of the chunk generation pipeline.
    /// </summary>
    /// <remarks>
    /// A stage schedules its work against the supplied dependency and may read from
    /// or append data to the shared generation state. Stages must return a JobHandle
    /// representing all work scheduled by the stage.
    /// </remarks>
    public interface IGenerationStage
    {
        /// <summary>
        /// Schedules this stage for the specified chunk generation state.
        /// </summary>
        /// <param name="state">Shared state being populated by the generation pipeline.</param>
        /// <param name="dependency">
        /// Job that must complete before this stage can safely execute.
        /// </param>
        /// <returns>
        /// A JobHandle representing this stage and its dependency chain.
        /// </returns>
        JobHandle Schedule(ChunkGenerationState state, JobHandle dependency);
    }
}