using _Project.Features.Persistence.Application;
using _Project.Features.ProceduralWorld.Application.Persistence;

namespace _Project.Features.Core.Application
{
    public interface IGameSaveService
    {
        void SaveAll();
    }

    public sealed class GameSaveService : IGameSaveService
    {
        private readonly IPlayerPersistence _playerPersistence;
        private readonly IWorldSaveService _worldSaveService;

        public GameSaveService(
            IPlayerPersistence playerPersistence,
            IWorldSaveService worldSaveService)
        {
            _playerPersistence = playerPersistence;
            _worldSaveService = worldSaveService;
        }

        public void SaveAll()
        {
            _playerPersistence.SavePlayer();
            _worldSaveService.SaveAllDirty();
        }
    }
}