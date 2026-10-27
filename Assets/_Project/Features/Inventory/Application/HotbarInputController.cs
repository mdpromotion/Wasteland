using System;
using _Project.Features.Core.Domain;
using _Project.Features.Core.Presentation;
using _Project.Features.Items.Domain;
using UnityEngine;

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
            Debug.Log("[HotbarInput] Controller subscribed to input events.");
        }

        private void OnHotbarSlotSelected(int slot)
        {
            Debug.Log(
                $"[HotbarInput] Controller received slot {slot + 1}; paused={_gameState.Paused}.");
            if (_gameState.Paused)
            {
                Debug.Log("[HotbarInput] Selection ignored because the game is paused.");
                return;
            }

            bool changed = _inventory.SelectSlot(slot);
            string itemName = _inventory.SelectedItem?.DisplayName ?? "<empty>";
            Debug.Log(
                $"[HotbarInput] Inventory selection processed; changed={changed}, " +
                $"selected slot={_inventory.SelectedSlot + 1}, item={itemName}.");
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
