using System;
using _Project.Features.Core.Domain;
using _Project.Features.Core.Presentation;
using VContainer.Unity;

namespace _Project.Features.Items.Application
{
    public sealed class PickupItemController : IInitializable, IDisposable
    {
        private readonly IPlayerPickupInputReader _inputReader;
        private readonly IGameState _gameState;
        private readonly IItemCollector _itemCollector;
        private bool _initialized;

        public PickupItemController(
            IPlayerPickupInputReader inputReader,
            IGameState gameState,
            IItemCollector itemCollector)
        {
            _inputReader = inputReader;
            _gameState = gameState;
            _itemCollector = itemCollector;
        }

        public void Initialize()
        {
            if (_initialized)
                return;

            _inputReader.PickupItemRequested += OnPickupItemRequested;
            _initialized = true;
        }

        private void OnPickupItemRequested()
        {
            if (!_gameState.Paused)
                _itemCollector.TryCollectNearestPickup();
        }

        public void Dispose()
        {
            if (!_initialized)
                return;

            _inputReader.PickupItemRequested -= OnPickupItemRequested;
            _initialized = false;
        }
    }
}
