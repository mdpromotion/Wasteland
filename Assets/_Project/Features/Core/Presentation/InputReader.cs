using System;
using UnityEngine;
using UnityEngine.InputSystem;
using IInitializable = VContainer.Unity.IInitializable;

namespace _Project.Features.Core.Presentation
{
    public interface IPlayerInputReader
    {
        bool LeftMouseButtonPressed { get; }
        bool RightMouseButtonPressed { get; }
        event Action<int> HotbarSlotSelected;
        event Action UseItemRequested;
        Vector2 Move { get; }
        Vector2 Look { get; }
        bool JumpPressed { get; }
        bool SprintPressed { get; }
        bool CrouchPressed { get; }
    }

    public interface IPlayerUIInputReader
    {
        event Action PauseClicked;
    }

    public interface IPlayerDropInputReader
    {
        event Action DropItemRequested;
    }

    public interface IPlayerPickupInputReader
    {
        event Action PickupItemRequested;
    }

    public sealed class InputReader :
        IPlayerInputReader,
        IPlayerUIInputReader,
        IPlayerDropInputReader,
        IPlayerPickupInputReader,
        IInitializable,
        IDisposable
    {
        private readonly InputSystem_Actions _inputActions;
        private readonly InputAction[] _hotbarSlotActions = new InputAction[9];
        private readonly Action<InputAction.CallbackContext>[] _hotbarSlotCallbacks =
            new Action<InputAction.CallbackContext>[9];
        private InputAction _dropItemAction;
        private bool _initialized;

        public InputReader(InputSystem_Actions inputActions)
        {
            _inputActions = inputActions;
        }

        public bool LeftMouseButtonPressed => _inputActions.Player.LeftMouse.IsPressed();
        public bool RightMouseButtonPressed => _inputActions.Player.RightMouse.IsPressed();
        public Vector2 Move => _inputActions.Player.Move.ReadValue<Vector2>();
        public Vector2 Look => _inputActions.Player.Look.ReadValue<Vector2>();
        public bool JumpPressed => _inputActions.Player.Jump.IsPressed();
        public bool SprintPressed => _inputActions.Player.Sprint.IsPressed();
        public bool CrouchPressed => _inputActions.Player.Crouch.IsPressed();

        public event Action<int> HotbarSlotSelected;
        public event Action UseItemRequested;
        
        public event Action PauseClicked;
        public event Action DropItemRequested;
        public event Action PickupItemRequested;

        public void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;
            _inputActions.Enable();

            for (int slot = 0; slot < _hotbarSlotActions.Length; slot++)
            {
                // Resolved through the generated wrapper (no string lookup at runtime).
                InputAction action = slot switch
                {
                    0 => _inputActions.Player.@HotbarSlot1,
                    1 => _inputActions.Player.@HotbarSlot2,
                    2 => _inputActions.Player.@HotbarSlot3,
                    3 => _inputActions.Player.@HotbarSlot4,
                    4 => _inputActions.Player.@HotbarSlot5,
                    5 => _inputActions.Player.@HotbarSlot6,
                    6 => _inputActions.Player.@HotbarSlot7,
                    7 => _inputActions.Player.@HotbarSlot8,
                    8 => _inputActions.Player.@HotbarSlot9,
                    _ => throw new InvalidOperationException($"Unsupported hotbar slot index {slot}.")
                };

                int selectedSlot = slot;
                Action<InputAction.CallbackContext> callback =
                    _ => OnHotbarSlotPerformed(selectedSlot);

                action.performed += callback;
                _hotbarSlotActions[selectedSlot] = action;
                _hotbarSlotCallbacks[selectedSlot] = callback;
            }

            _inputActions.Player.RightMouse.performed += OnRightMousePerformed;
            _dropItemAction = _inputActions.Player.Get().FindAction("DropItem", true);
            _dropItemAction.performed += OnDropItemPerformed;
            _inputActions.Player.Interact.started += OnPickupStarted;
            _inputActions.UI.InGameMenu.canceled += OnPauseReleased;
        }

        private void OnHotbarSlotPerformed(int selectedSlot)
        {
            HotbarSlotSelected?.Invoke(selectedSlot);
        }

        private void OnRightMousePerformed(InputAction.CallbackContext context)
        {
            UseItemRequested?.Invoke();
        }

        private void OnPauseReleased(InputAction.CallbackContext context)
        {
            PauseClicked?.Invoke();
        }

        private void OnDropItemPerformed(InputAction.CallbackContext context)
        {
            DropItemRequested?.Invoke();
        }

        private void OnPickupStarted(InputAction.CallbackContext context)
        {
            PickupItemRequested?.Invoke();
        }

        public void Dispose()
        {
            _inputActions.Player.RightMouse.performed -= OnRightMousePerformed;
            if (_dropItemAction != null)
                _dropItemAction.performed -= OnDropItemPerformed;
            _dropItemAction = null;
            _inputActions.Player.Interact.started -= OnPickupStarted;
            _inputActions.UI.InGameMenu.canceled -= OnPauseReleased;

            for (int slot = 0; slot < _hotbarSlotActions.Length; slot++)
            {
                if (_hotbarSlotActions[slot] != null && _hotbarSlotCallbacks[slot] != null)
                    _hotbarSlotActions[slot].performed -= _hotbarSlotCallbacks[slot];

                _hotbarSlotActions[slot] = null;
                _hotbarSlotCallbacks[slot] = null;
            }

            _initialized = false;
            _inputActions.Disable();
            _inputActions.Dispose();
        }
    }
}
