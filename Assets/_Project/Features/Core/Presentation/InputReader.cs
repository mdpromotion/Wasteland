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

    public sealed class InputReader :
        IPlayerInputReader,
        IPlayerUIInputReader,
        IInitializable,
        IDisposable
    {
        private readonly InputSystem_Actions _inputActions;
        private readonly InputAction[] _hotbarSlotActions = new InputAction[9];
        private readonly Action<InputAction.CallbackContext>[] _hotbarSlotCallbacks =
            new Action<InputAction.CallbackContext>[9];
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

        public void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;
            _inputActions.Enable();

            for (int slot = 1; slot <= 9; slot++)
            {
                int selectedSlot = slot - 1;
                InputAction action = _inputActions.Player.Get().FindAction($"HotbarSlot{slot}", true);
                Action<InputAction.CallbackContext> callback =
                    context => OnHotbarSlotPerformed(context, action, selectedSlot);

                action.performed += callback;
                _hotbarSlotActions[selectedSlot] = action;
                _hotbarSlotCallbacks[selectedSlot] = callback;
                Debug.Log(
                    $"[HotbarInput] Registered {action.name}, enabled={action.enabled}, binding={action.bindings[0].effectivePath}.");
            }

            _inputActions.Player.RightMouse.performed += OnRightMousePerformed;
            _inputActions.UI.InGameMenu.canceled += OnPauseReleased;
            Debug.Log("[HotbarInput] InputReader initialized.");
        }

        private void OnHotbarSlotPerformed(
            InputAction.CallbackContext context,
            InputAction action,
            int selectedSlot)
        {
            int listeners = HotbarSlotSelected?.GetInvocationList().Length ?? 0;
            Debug.Log(
                $"[HotbarInput] {context.control.path} performed {action.name}; " +
                $"emitting slot {selectedSlot + 1} to {listeners} listener(s).");
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

        public void Dispose()
        {
            _inputActions.Player.RightMouse.performed -= OnRightMousePerformed;
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
