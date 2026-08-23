using System;

namespace _Project.Features.ProceduralWorld.Domain.Chunks
{
    public sealed class ChunkGenerationResult : IDisposable
    {
        public ChunkGenerationState State { get; }

        public ChunkGenerationResult(ChunkGenerationState state)
        {
            State = state;
        }
        
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