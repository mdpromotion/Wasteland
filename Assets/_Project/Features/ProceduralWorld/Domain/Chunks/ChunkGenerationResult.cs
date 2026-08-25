using System;

namespace _Project.Features.ProceduralWorld.Domain.Chunks
{
    /// <summary>
    /// Represents the completed output of a chunk generation task.
    /// </summary>
    /// <remarks>
    /// The result exposes the generation state after the scheduled job chain has completed.
    /// Ownership of the disposable water-related Native containers is transferred to this
    /// result until <see cref="Dispose"/> is called.
    /// </remarks>
    public sealed class ChunkGenerationResult : IDisposable
    {
        /// <summary>
        /// State accumulated by the generation pipeline.
        /// </summary>
        public ChunkGenerationState State { get; }

        public ChunkGenerationResult(ChunkGenerationState state)
        {
            State = state;
        }
        
        /// <summary>
        /// Releases Native containers owned by the generation result.
        /// </summary>
        public void Dispose()
        {
            if(State.WaterMaskPixels.IsCreated)
                State.WaterMaskPixels.Dispose();

            if(State.WaterBounds.IsCreated)
                State.WaterBounds.Dispose();

            if(State.WaterAverageHeight.IsCreated)
                State.WaterAverageHeight.Dispose();
        }
    }
}