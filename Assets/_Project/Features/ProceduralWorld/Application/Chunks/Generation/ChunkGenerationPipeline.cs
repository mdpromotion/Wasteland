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
    
    public sealed class ChunkGenerationPipeline : IChunkGenerator
    {
        private readonly List<IGenerationStage> _stages = new();

        public void Add(IGenerationStage stage)
        {
            _stages.Add(stage);
        }
        

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