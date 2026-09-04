using System;
using _Project.Features.GameTime.Application;
using _Project.Features.Persistence.Application;
using _Project.Features.Player.Application;
using _Project.Features.ProceduralWorld.Application.Chunks;
using _Project.Features.UI.Infrastructure;
using Cysharp.Threading.Tasks;

namespace _Project.Features.Core.Application
{
    public sealed class GameLoadService
    {
        private readonly IPlayerController _player;
        private readonly IGamePersistenceCoordinator _gamePersistenceCoordinator;
        private readonly IPlayerPositionService _positionService;
        private readonly IGameTimeSaveService _gameTimeSaveService;
        private readonly IChunkManager _chunkManager;

        public GameLoadService(
            IPlayerController player,
            IGamePersistenceCoordinator gamePersistenceCoordinator,
            IPlayerPositionService positionService,
            IGameTimeSaveService gameTimeSaveService,
            IChunkManager chunkManager)
        {
            _player = player;
            _gamePersistenceCoordinator = gamePersistenceCoordinator;
            _positionService = positionService;
            _gameTimeSaveService = gameTimeSaveService;
            _chunkManager = chunkManager;
        }

        public async UniTask LoadAsync()
        {
            _player.Freeze(true);

            _gameTimeSaveService.Load();
            
            if (_gamePersistenceCoordinator.TryLoadGame())
                await WaitForPlayerChunkAsync();
            else
                await PrepareNewPlayerAsync();

            _player.Freeze(false);
        }

        private async UniTask WaitForPlayerChunkAsync()
        {
            var targetChunk = _positionService.GetCurrentChunkCoordinate();

            while (!_chunkManager.IsChunkLoaded(targetChunk))
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
        }

        private async UniTask PrepareNewPlayerAsync()
        {
            while (!_chunkManager.IsReady)
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

            while (!_player.Prepare())
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            _player.Ready();
        }
    }
}