using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Infrastructure.Chunks;

namespace _Project.Features.ProceduralWorld.Application.Interfaces
{
    public interface IChunkLookup
    {
        bool Contains(ChunkCoordinate coordinate);
        bool TryGet(ChunkCoordinate coordinate, out ChunkInstance chunk);
        ChunkInstance Get(ChunkCoordinate coordinate);
    }
}