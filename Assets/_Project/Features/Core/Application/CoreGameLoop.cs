using System;
using _Project.Features.Core.Domain;
using _Project.Features.Cursor.Presentation;
using _Project.Features.Persistence.Application;
using _Project.Features.Player.Application;
using _Project.Features.ProceduralWorld.Application.Chunks;
using _Project.Features.UI.Infrastructure;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Features.Core.Application
{
    public class CoreGameLoop : IInitializable, IDisposable
    {
        private readonly IPlayerController _player;
        private readonly SceneTransitionService _sceneTransitionService;
        private readonly IGameState _gameState;
        private readonly IGameStateController _gameStateController;
        private readonly ICursorService _cursorService;
        private readonly IChunkManager _chunkManager;
        private readonly IPlayerPersistence _playerPersistence;
        private readonly IPlayerPositionService _positionService;
        private readonly IGameSessionSaveController _saveController;

        public CoreGameLoop(
            IPlayerController player,
            SceneTransitionService sceneTransitionService,
            IGameState gameState,
            IGameStateController gameStateController,
            ICursorService cursorService,
            IChunkManager chunkManager,
            IPlayerPersistence playerPersistence,
            IPlayerPositionService positionService,
            IGameSessionSaveController saveController)
        {
            _player = player;
            _sceneTransitionService = sceneTransitionService;
            _gameState = gameState;
            _gameStateController = gameStateController;
            _cursorService = cursorService;
            _chunkManager = chunkManager;
            _playerPersistence = playerPersistence;
            _positionService = positionService;
            _saveController = saveController;
        }

        public void Initialize()
        {
            InitializeAsync().Forget();
            _gameStateController.SetPaused(false);
            _cursorService.LockCursor(true);

            _gameState.PausedChanged += OnPausedChanged;
        }

        private void OnPausedChanged(bool state)
        {
            _cursorService.LockCursor(!state);
        }

        private async UniTaskVoid InitializeAsync()
        {
            _player.Freeze(true);

            if (_playerPersistence.TryLoadPlayer())
            {
                var targetChunk = _positionService.GetCurrentChunkCoordinate();

                while (!_chunkManager.IsChunkLoaded(targetChunk))
                    await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
            }
            else
            {
                while (!_chunkManager.IsReady)
                    await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

                while (!_player.Prepare())
                    await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

                _player.Ready();
            }

            _player.Freeze(false);
            _saveController.ArmAutoSave();

            await _sceneTransitionService.CompleteAsync();
        }

        public void Dispose()
        {
            _saveController.SaveOnExit();
            _gameState.PausedChanged -= OnPausedChanged;
        }
    }
}