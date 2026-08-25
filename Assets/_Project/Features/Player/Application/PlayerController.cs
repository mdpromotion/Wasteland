using System;
using _Project.Features.Core.Domain;
using _Project.Features.Core.Presentation;
using _Project.Features.Player.Presentation;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Features.Player.Application
{
    public class PlayerController : IFixedTickable, IPlayerController
    {
        private float _yaw;
        private float _pendingYawDelta;

        public float LookYaw => _yaw;

        private float _lastVerticalVelocity;
        private bool _wasGrounded;
        private bool _isFrozen;

        private readonly IMovementMode _groundMovement;
        private readonly IMovementMode _waterMovement;

        private readonly IPlayerInputReader _input;
        private readonly IFpsPlayerMotor _playerMotor;
        private readonly PlayerEnvironmentState _environmentState;
        private readonly IGameState _gameState;

        private Vector3 _safePosition;

        private const float LandingFallSpeedThreshold = -3f;

        public event Action OnJumped;
        public event Action OnLanded;

        public PlayerController(
            IFpsPlayerMotor playerMotor,
            GroundMovementUseCase groundMovement,
            SwimmingMovementUseCase waterMovement,
            IPlayerInputReader input,
            PlayerEnvironmentState environmentState,
            IGameState gameState)
        {
            _playerMotor = playerMotor;
            _groundMovement = groundMovement;
            _waterMovement = waterMovement;
            _input = input;
            _environmentState = environmentState;
            _gameState = gameState;
        }

        public bool Prepare()
        {
            return _playerMotor.TryGetSafeGroundPosition(out _safePosition);
        }

        public void Ready()
        {
            _playerMotor.TeleportToPosition(_safePosition);
        }

        public void FixedTick()
        {
            if (_gameState.Paused)
                return;

            _environmentState.Update();

            bool swimming = _environmentState.IsInWater;
            bool groundedNow = _environmentState.IsGrounded && !swimming;

            _yaw += _pendingYawDelta;
            _pendingYawDelta = 0f;

            Quaternion rotation = Quaternion.Euler(0f, _yaw, 0f);

            _playerMotor.SetRotation(rotation);

            if (_isFrozen)
                return;

            Vector3 forward = rotation * Vector3.forward;
            Vector3 right = rotation * Vector3.right;

            Vector3 velocity = _playerMotor.CurrentVelocity;

            IMovementMode movementMode = swimming
                ? _waterMovement
                : _groundMovement;

            Vector3 targetVelocity =
                movementMode.BuildVelocity(_input.Move, forward, right, velocity);

            if (swimming)
            {
                if (_input.JumpPressed)
                {
                    movementMode.TryJump(ref targetVelocity);
                }

                if (_input.CrouchPressed)
                {
                    movementMode.TryCrouch(ref targetVelocity);
                }
            }
            else
            {
                if (_input.JumpPressed && groundedNow)
                {
                    if (movementMode.TryJump(ref targetVelocity))
                    {
                        OnJumped?.Invoke();
                    }
                }
            }

            if (_input.CrouchPressed)
            {
                movementMode.TryCrouch(ref targetVelocity);
            }

            if (groundedNow &&
                !_wasGrounded &&
                _lastVerticalVelocity <= LandingFallSpeedThreshold)
            {
                OnLanded?.Invoke();
            }

            _wasGrounded = groundedNow;
            _lastVerticalVelocity = velocity.y;

            _playerMotor.SetVelocity(targetVelocity);
        }

        public void SetLookYaw(float yawDelta)
        {
            _pendingYawDelta += yawDelta;
        }

        public void Freeze(bool state)
        {
            _isFrozen = state;
            _playerMotor.Freeze(_isFrozen);
        }
    }
}