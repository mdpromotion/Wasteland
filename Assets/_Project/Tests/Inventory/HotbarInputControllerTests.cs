using System;
using _Project.Features.Core.Domain;
using _Project.Features.Core.Presentation;
using _Project.Features.Items.Application;
using _Project.Features.Items.Domain;
using NUnit.Framework;
using UnityEngine;

namespace _Project.Tests.Items
{
    public sealed class HotbarInputControllerTests
    {
        private Inventory _inventory;
        private GameState _gameState;
        private FakePlayerInputReader _inputReader;
        private HotbarInputController _controller;

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
            _inputReader = new FakePlayerInputReader();
            _controller = new HotbarInputController(_inputReader, _gameState, _inventory);
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

            Assert.That(_inventory.SelectedSlot, Is.EqualTo(0));
            Assert.That(_inventory.GetSlot(0).Count, Is.EqualTo(2));
        }

        [Test]
        public void UseItemRequested_ConsumesSelectedApple()
        {
            _inventory.TryAdd(ItemId.Apple, 2);

            _inputReader.RaiseUseItemRequested();

            Assert.That(_inventory.GetSlot(0).Count, Is.EqualTo(1));
        }

        private sealed class FakePlayerInputReader : IPlayerInputReader
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
    }
}
