using _Project.Features.ProceduralWorld.Domain.Chunks;

namespace _Project.Features.Persistence.Application
{
    public interface IChunkPersistence
    {
        void SaveWorldState();
        void InitializeOrigin(ChunkCoordinate origin);
    }

    public sealed class ChunkPersistenceService : IChunkPersistence
    {
        private readonly ChunkGrid _grid;

        public ChunkPersistenceService(ChunkGrid grid)
        {
            _grid = grid;
        }

        public void SaveWorldState()
        {
        }

        public void InitializeOrigin(ChunkCoordinate origin)
        {
            _grid.SetOriginCoordinate(origin);
        }
    }
}