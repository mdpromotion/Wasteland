using System;
using _Project.Features.Core.Domain;
using _Project.Features.Core.Presentation;
using _Project.Features.Items.Application;
using _Project.Features.Items.Domain;
using NUnit.Framework;
using UnityEngine;

namespace _Project.Tests.Items
{
    public sealed class InventoryInputControllerTests
    {
        private Inventory _inventory;
        private GameState _gameState;
        private TestHudReader _inputReader;
        private TestPickupReader _pickupReader;
        private TestDropReader _dropReader;
        private TestItemCollector _collector;
        private TestItemDropService _dropService;
        private InventoryInputController _controller;

        [SetUp]
        public void SetUp()
        {
            var catalog = new ItemCatalog(new[]
            {
                new ItemDefinition(
                    ItemId.IronAxe,
                    "Iron Axe",
                    1,
                    new ItemBehavior[] { new TreeChoppingItemBehavior(1f) }),
                new ItemDefinition(ItemId.Wood, "Wood", 50),
                new ItemDefinition(ItemId.Apple, "Apple", 50, new ItemBehavior[] { new ConsumableItemBehavior() })
            });

            _inventory = new Inventory(catalog);
            _gameState = new GameState();
            _inputReader = new TestHudReader();
            _pickupReader = new TestPickupReader();
            _dropReader = new TestDropReader();
            _collector = new TestItemCollector();
            _dropService = new TestItemDropService();
            _controller = new InventoryInputController(
                _inputReader,
                _pickupReader,
                _dropReader,
                _gameState,
                _inventory,
                _collector,
                _dropService);
            _controller.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public void HotbarSlotSelected_SelectsRequestedSlot()
        {
            _inventory.TryAdd(ItemId.IronAxe, 1);
            _inventory.TryAdd(ItemId.Wood, 1);

            _inputReader.RaiseHotbarSlotSelected(1);

            Assert.That(_inventory.SelectedSlot, Is.EqualTo(1));
            Assert.That(_inventory.SelectedItem.Id, Is.EqualTo(ItemId.Wood));
        }

        [Test]
        public void PausedGame_IgnoresHotbarAndUseRequests()
        {
            _inventory.TryAdd(ItemId.Apple, 2);
            _gameState.SetPaused(true);

            _inputReader.RaiseHotbarSlotSelected(1);
            _inputReader.RaiseUseItemRequested();
            _pickupReader.RaisePickupRequested();
            _dropReader.RaiseDropRequested();

            Assert.That(_inventory.SelectedSlot, Is.EqualTo(0));
            Assert.That(_inventory.GetSlot(0).Count, Is.EqualTo(2));
            Assert.That(_collector.Requests, Is.EqualTo(0));
            Assert.That(_dropService.Requests, Is.EqualTo(0));
        }

        [Test]
        public void UseItemRequested_ConsumesSelectedApple()
        {
            _inventory.TryAdd(ItemId.Apple, 2);

            _inputReader.RaiseUseItemRequested();

            Assert.That(_inventory.GetSlot(0).Count, Is.EqualTo(1));
        }

        [Test]
        public void PickupItemRequested_CollectsNearestOnce()
        {
            _pickupReader.RaisePickupRequested();
            _pickupReader.RaisePickupRequested();

            Assert.That(_collector.Requests, Is.EqualTo(2));
        }

        [Test]
        public void DropItemRequested_DropsSelectedOnce()
        {
            _dropReader.RaiseDropRequested();

            Assert.That(_dropService.Requests, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_UnsubscribesFromAllInputs()
        {
            _controller.Dispose();

            _inventory.TryAdd(ItemId.IronAxe, 1);
            _inputReader.RaiseHotbarSlotSelected(1);
            _inputReader.RaiseUseItemRequested();
            _pickupReader.RaisePickupRequested();
            _dropReader.RaiseDropRequested();

            Assert.That(_inventory.SelectedSlot, Is.EqualTo(0));
            Assert.That(_collector.Requests, Is.EqualTo(0));
            Assert.That(_dropService.Requests, Is.EqualTo(0));
        }

        [Test]
        public void Dispose_CalledTwice_IsSafe()
        {
            _controller.Dispose();
            Assert.DoesNotThrow(() => _controller.Dispose());
        }

        private sealed class TestHudReader : IPlayerInputReader
        {
            public bool LeftMouseButtonPressed => false;
            public bool RightMouseButtonPressed => false;
            public event Action<int> HotbarSlotSelected;
            public event Action UseItemRequested;
            public Vector2 Move => Vector2.zero;
            public Vector2 Look => Vector2.zero;
            public bool JumpPressed => false;
            public bool SprintPressed => false;
            public bool CrouchPressed => false;

            public void RaiseHotbarSlotSelected(int slot) => HotbarSlotSelected?.Invoke(slot);
            public void RaiseUseItemRequested() => UseItemRequested?.Invoke();
        }

        private sealed class TestPickupReader : IPlayerPickupInputReader
        {
            public event Action PickupItemRequested;

            public void RaisePickupRequested() => PickupItemRequested?.Invoke();
        }

        private sealed class TestDropReader : IPlayerDropInputReader
        {
            public event Action DropItemRequested;

            public void RaiseDropRequested() => DropItemRequested?.Invoke();
        }

        private sealed class TestItemCollector : IItemCollector
        {
            public int Requests { get; private set; }

            public bool TryCollect(ItemId itemId)
            {
                Requests++;
                return true;
            }

            public void RegisterNearbyPickup(IItemPickupTarget pickup) { }
            public void UnregisterNearbyPickup(IItemPickupTarget pickup) { }
            public bool TryCollectNearestPickup()
            {
                Requests++;
                return true;
            }
        }

        private sealed class TestItemDropService : IItemDropService
        {
            public int Requests { get; private set; }

            public bool TryDropSelected()
            {
                Requests++;
                return true;
            }
        }
    }
}
