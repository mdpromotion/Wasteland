using _Project.Features.ProceduralWorld.Domain.Chunks;


namespace _Project.Features.ProceduralWorld.Application.Interfaces
{
    public interface IChunkNeighborConnector
    {
        void Connect(IChunkLookup chunks, ChunkCoordinate coordinate);


        void Disconnect(IChunkLookup chunks, ChunkCoordinate coordinate);
    }
}