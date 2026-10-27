using System;
using _Project.Features.Core.Domain;
using _Project.Features.Core.Presentation;
using VContainer.Unity;

namespace _Project.Features.Items.Application
{
    public sealed class DropItemController : IInitializable, IDisposable
    {
        private readonly IPlayerDropInputReader _inputReader;
        private readonly IGameState _gameState;
        private readonly IItemDropService _itemDropService;
        private bool _initialized;

        public DropItemController(
            IPlayerDropInputReader inputReader,
            IGameState gameState,
            IItemDropService itemDropService)
        {
            _inputReader = inputReader;
            _gameState = gameState;
            _itemDropService = itemDropService;
        }

        public void Initialize()
        {
            if (_initialized)
                return;

            _inputReader.DropItemRequested += OnDropItemRequested;
            _initialized = true;
        }

        private void OnDropItemRequested()
        {
            if (!_gameState.Paused)
                _itemDropService.TryDropSelected();
        }

        public void Dispose()
        {
            if (!_initialized)
                return;

            _inputReader.DropItemRequested -= OnDropItemRequested;
            _initialized = false;
        }
    }
}
