using _Project.Features.ProceduralWorld.Domain.Chunks;
using Unity.Jobs;

namespace _Project.Features.ProceduralWorld.Domain
{
    /// <summary>
    /// Tracks a scheduled chunk generation job and the state produced by its generation stages.
    /// </summary>
    /// <remarks>
    /// The task does not execute or dispose the generation state itself. The scheduler is
    /// responsible for completing the job and disposing the state when generation is cancelled,
    /// or transferring the state to <see cref="ChunkGenerationResult"/> when generation succeeds.
    /// </remarks>
    public sealed class GenerationTask
    {
        /// <summary>
        /// Job handle representing the complete dependency chain of the generation pipeline.
        /// </summary>
        public JobHandle Handle { get; }

        /// <summary>
        /// Mutable generation state shared by all stages in the pipeline.
        /// </summary>
        public ChunkGenerationState State { get; }

        /// <summary>
        /// Indicates that the generated result must be discarded when the job completes.
        /// </summary>
        public bool Cancelled;
    }
}