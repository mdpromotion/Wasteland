using _Project.Features.Core.Presentation;
using _Project.Features.Player.Domain;
using _Project.Features.Player.Infrastructure;
using UnityEngine;

namespace _Project.Features.Player.Application.UseCases
{
    public sealed class GroundMovementUseCase : IMovementMode
    {
        private readonly PlayerMovementConfig _config;
        private readonly IPlayerStanceState _stance;
        private readonly IPlayerInputReader _input;


        public GroundMovementUseCase(
            PlayerMovementConfig config,
            IPlayerStanceState stance,
            IPlayerInputReader input)
        {
            _config = config;
            _stance = stance;
            _input = input;
        }


        public Vector3 BuildVelocity(Vector2 moveInput, Vector3 forward, Vector3 right, Vector3 currentVelocity)
        {
            Vector3 planarForward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
            Vector3 planarRight = Vector3.ProjectOnPlane(right, Vector3.up).normalized;

            Vector3 move = planarForward * moveInput.y + planarRight * moveInput.x;

            if (move.sqrMagnitude > 1f) 
                move.Normalize();
            
            float speedMultiplier = GetSpeedMultiplier();
            
            Vector3 desiredVelocity = move * (_config.BaseSpeed * speedMultiplier);
            
            return new Vector3(desiredVelocity.x, currentVelocity.y, desiredVelocity.z);
        }


        public bool TryJump(ref Vector3 velocity)
        {
            velocity.y = _config.JumpVelocity;

            return true;
        }


        public bool TryCrouch(ref Vector3 velocity)
        {
            return false;
        }


        private float GetSpeedMultiplier()
        {
            if (_stance != null && _stance.IsCrouching)
            {
                return _config.CrouchSpeedMultiplier;
            }


            if (_input.SprintPressed)
            {
                return _config.SprintSpeedMultiplier;
            }


            return 1f;
        }
    }
}