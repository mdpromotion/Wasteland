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
        
        public event Action PauseClicked;

        public void Initialize()
        {
            _inputActions.Enable();
            
            _inputActions.UI.InGameMenu.canceled += OnPauseReleased;
        }

        private void OnPauseReleased(InputAction.CallbackContext context)
        {
            PauseClicked?.Invoke();
        }

        public void Dispose()
        {
            _inputActions.UI.InGameMenu.canceled -= OnPauseReleased;
            
            _inputActions.Disable();
            _inputActions.Dispose();
        }
    }
}