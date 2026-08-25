using _Project.Features.Player.Infrastructure;
using UnityEngine;

namespace _Project.Features.Player.Application.UseCases
{
    public sealed class SwimmingMovementUseCase : IMovementMode
    {
        private readonly PlayerMovementConfig _config;

        public SwimmingMovementUseCase(
            PlayerMovementConfig config)
        {
            _config = config;
        }

        public Vector3 BuildVelocity(
            Vector2 input,
            Vector3 forward,
            Vector3 right,
            Vector3 currentVelocity)
        {
            Vector3 move =
                forward * input.y +
                right * input.x;

            if (move.sqrMagnitude > 1f)
                move.Normalize();

            Vector3 targetVelocity =
                move * _config.BaseSpeed;


            Vector3 horizontalVelocity =
                new Vector3(
                    currentVelocity.x,
                    0f,
                    currentVelocity.z);


            horizontalVelocity =
                Vector3.Lerp(
                    horizontalVelocity,
                    targetVelocity,
                    _config.WaterAcceleration *
                    Time.fixedDeltaTime);


            float verticalVelocity =
                currentVelocity.y;


            verticalVelocity +=
                _config.WaterGravity *
                Time.fixedDeltaTime;


            verticalVelocity =
                Mathf.Lerp(
                    verticalVelocity,
                    0f,
                    _config.WaterVerticalDrag *
                    Time.fixedDeltaTime);


            return new Vector3(
                horizontalVelocity.x,
                verticalVelocity,
                horizontalVelocity.z);
        }


        public bool TryJump(
            ref Vector3 velocity)
        {
            velocity.y =
                _config.SwimVerticalSpeed;

            return true;
        }


        public bool TryCrouch(
            ref Vector3 velocity)
        {
            velocity.y =
                -_config.SwimVerticalSpeed;

            return true;
        }
    }
}