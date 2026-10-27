using System;
using _Project.Features.Core.Domain;
using _Project.Features.Core.Presentation;
using _Project.Features.Items.Domain;

namespace _Project.Features.Items.Application
{
    public sealed class HotbarInputController : IDisposable
    {
        private readonly IPlayerInputReader _inputReader;
        private readonly IGameState _gameState;
        private readonly Inventory _inventory;

        public HotbarInputController(
            IPlayerInputReader inputReader,
            IGameState gameState,
            Inventory inventory)
        {
            _inputReader = inputReader;
            _gameState = gameState;
            _inventory = inventory;

            _inputReader.HotbarSlotSelected += OnHotbarSlotSelected;
            _inputReader.UseItemRequested += OnUseItemRequested;
        }

        private void OnHotbarSlotSelected(int slot)
        {
            if (!_gameState.Paused)
                _inventory.SelectSlot(slot);
        }

        private void OnUseItemRequested()
        {
            if (!_gameState.Paused)
                _inventory.TryConsumeSelected();
        }

        public void Dispose()
        {
            _inputReader.HotbarSlotSelected -= OnHotbarSlotSelected;
            _inputReader.UseItemRequested -= OnUseItemRequested;
        }
    }
}
