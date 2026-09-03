using _Project.Features.GameTime.Application;
using _Project.Features.Persistence.Application;
using _Project.Features.ProceduralWorld.Application.Persistence;

namespace _Project.Features.Core.Application
{
    public interface IGameSaveService
    {
        void SaveAll();
        void SavePlayer();
        void SaveWorld();
        void SaveGameTime();
    }

    public sealed class GameSaveService : IGameSaveService
    {
        private readonly IPlayerPersistence _playerPersistence;
        private readonly IWorldSaveService _worldSaveService;
        private readonly IGameTimeSaveService _gameTimeSaveService;

        public GameSaveService(
            IPlayerPersistence playerPersistence,
            IWorldSaveService worldSaveService,
            IGameTimeSaveService gameTimeSaveService)
        {
            _playerPersistence = playerPersistence;
            _worldSaveService = worldSaveService;
            _gameTimeSaveService = gameTimeSaveService;
        }

        public void SaveAll()
        {
            SavePlayer();
            SaveWorld();
            SaveGameTime();
        }

        public void SavePlayer()
        {
            _playerPersistence.SavePlayer();
        }

        public void SaveWorld()
        {
            _worldSaveService.SaveAllDirty();
        }

        public void SaveGameTime()
        {
            _gameTimeSaveService.Save();
        }
    }
}