using System;
using _Project.Features.Core.Domain;
using _Project.Features.Core.Presentation;
using _Project.Features.Items.Domain;
using VContainer.Unity;

namespace _Project.Features.Items.Application
{
    /// <summary>
    /// Single wiring point for all item-related player input (hotbar selection, use,
    /// pickup, drop). Each behavior is gated by the paused game state.
    /// </summary>
    public sealed class InventoryInputController : IInitializable, IDisposable
    {
        private readonly IPlayerInputReader _inputReader;
        private readonly IPlayerPickupInputReader _pickupInputReader;
        private readonly IPlayerDropInputReader _dropInputReader;
        private readonly IGameState _gameState;
        private readonly Inventory _inventory;
        private readonly IItemCollector _itemCollector;
        private readonly IItemDropService _itemDropService;
        private bool _initialized;

        public InventoryInputController(
            IPlayerInputReader inputReader,
            IPlayerPickupInputReader pickupInputReader,
            IPlayerDropInputReader dropInputReader,
            IGameState gameState,
            Inventory inventory,
            IItemCollector itemCollector,
            IItemDropService itemDropService)
        {
            _inputReader = inputReader ?? throw new ArgumentNullException(nameof(inputReader));
            _pickupInputReader = pickupInputReader ?? throw new ArgumentNullException(nameof(pickupInputReader));
            _dropInputReader = dropInputReader ?? throw new ArgumentNullException(nameof(dropInputReader));
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _itemCollector = itemCollector ?? throw new ArgumentNullException(nameof(itemCollector));
            _itemDropService = itemDropService ?? throw new ArgumentNullException(nameof(itemDropService));
        }

        public void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;
            _inputReader.HotbarSlotSelected += OnHotbarSlotSelected;
            _inputReader.UseItemRequested += OnUseItemRequested;
            _pickupInputReader.PickupItemRequested += OnPickupItemRequested;
            _dropInputReader.DropItemRequested += OnDropItemRequested;
        }

        private void OnHotbarSlotSelected(int slot)
        {
            if (_gameState.Paused)
                return;

            _inventory.SelectSlot(slot);
        }

        private void OnUseItemRequested()
        {
            if (_gameState.Paused)
                return;

            _inventory.TryConsumeSelected();
        }

        private void OnPickupItemRequested()
        {
            if (_gameState.Paused)
                return;

            _itemCollector.TryCollectNearestPickup();
        }

        private void OnDropItemRequested()
        {
            if (_gameState.Paused)
                return;

            _itemDropService.TryDropSelected();
        }

        public void Dispose()
        {
            if (!_initialized)
                return;

            _initialized = false;
            _inputReader.HotbarSlotSelected -= OnHotbarSlotSelected;
            _inputReader.UseItemRequested -= OnUseItemRequested;
            _pickupInputReader.PickupItemRequested -= OnPickupItemRequested;
            _dropInputReader.DropItemRequested -= OnDropItemRequested;
        }
    }
}
