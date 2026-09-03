using System;
using _Project.Features.Core.Domain;
using _Project.Features.Cursor.Presentation;
using _Project.Features.Persistence.Application;
using _Project.Features.UI.Infrastructure;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace _Project.Features.Core.Application
{
    public class CoreGameLoop : IInitializable, IDisposable
    {
        private readonly GameLoadService _gameLoadService;
        private readonly SceneTransitionService _sceneTransitionService;
        private readonly IGameState _gameState;
        private readonly IGameStateController _gameStateController;
        private readonly ICursorService _cursorService;
        private readonly IGameSessionSaveController _saveController;

        public CoreGameLoop(
            GameLoadService gameLoadService,
            SceneTransitionService sceneTransitionService,
            IGameState gameState,
            IGameStateController gameStateController,
            ICursorService cursorService,
            IGameSessionSaveController saveController)
        {
            _gameLoadService = gameLoadService;
            _sceneTransitionService = sceneTransitionService;
            _gameState = gameState;
            _gameStateController = gameStateController;
            _cursorService = cursorService;
            _saveController = saveController;
        }

        public void Initialize()
        {
            InitializeAsync().Forget();

            _gameStateController.SetPaused(false);
            _cursorService.LockCursor(true);

            _gameState.PausedChanged += OnPausedChanged;
        }

        private async UniTaskVoid InitializeAsync()
        {
            await _gameLoadService.LoadAsync();

            _saveController.ArmAutoSave();

            await _sceneTransitionService.CompleteAsync();
        }

        private void OnPausedChanged(bool state)
        {
            _cursorService.LockCursor(!state);
        }

        public void Dispose()
        {
            _saveController.SaveOnExit();
            _gameState.PausedChanged -= OnPausedChanged;
        }
    }
}