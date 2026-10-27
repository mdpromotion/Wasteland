using System;
using _Project.Features.Core.Domain;
using _Project.Features.Core.Presentation;
using _Project.Features.Items.Domain;
using VContainer.Unity;

namespace _Project.Features.Items.Application
{
    public sealed class HotbarInputController : IInitializable, IDisposable
    {
        private readonly IPlayerInputReader _inputReader;
        private readonly IGameState _gameState;
        private readonly Inventory _inventory;
        private bool _initialized;

        public HotbarInputController(
            IPlayerInputReader inputReader,
            IGameState gameState,
            Inventory inventory)
        {
            _inputReader = inputReader;
            _gameState = gameState;
            _inventory = inventory;
            Subscribe();
        }

        public void Initialize()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            if (_initialized)
                return;

            _inputReader.HotbarSlotSelected += OnHotbarSlotSelected;
            _inputReader.UseItemRequested += OnUseItemRequested;
            _initialized = true;
        }

        private void OnHotbarSlotSelected(int slot)
        {
            if (_gameState.Paused)
                return;

            _inventory.SelectSlot(slot);
        }

        private void OnUseItemRequested()
        {
            if (!_gameState.Paused)
                _inventory.TryConsumeSelected();
        }

        public void Dispose()
        {
            if (!_initialized)
                return;

            _inputReader.HotbarSlotSelected -= OnHotbarSlotSelected;
            _inputReader.UseItemRequested -= OnUseItemRequested;
            _initialized = false;
        }
    }
}
