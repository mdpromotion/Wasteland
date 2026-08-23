using _Project.Features.ProceduralWorld.Domain.Chunks;
using Unity.Jobs;

namespace _Project.Features.ProceduralWorld.Domain
{
    public sealed class GenerationTask
    {
        public JobHandle Handle { get; }
        public ChunkGenerationState State { get; }
        
        public bool Cancelled;
        
        public GenerationTask(
            JobHandle handle,
            ChunkGenerationState state)
        {
            Handle = handle;
            State = state;
        }
    }
}