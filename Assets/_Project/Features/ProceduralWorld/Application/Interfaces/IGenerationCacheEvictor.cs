using _Project.Features.ProceduralWorld.Domain.Chunks;

namespace _Project.Features.ProceduralWorld.Application.Interfaces
{
    public interface IGenerationCacheEvictor
    {
        void EvictOutside(ChunkCoordinate center, int viewDistance);
    }
}