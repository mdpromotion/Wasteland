using _Project.Features.Player.Domain;
using _Project.Features.Player.Presentation;
using UnityEngine;

namespace _Project.Features.Player.Application
{
    public interface IPlayerEnvironmentState
    {
        bool IsGrounded { get; }
        bool IsInWater { get; }
    }
    public sealed class PlayerEnvironmentState : IPlayerEnvironmentState
    {
        private const float GroundCheckRate = 10f;

        private readonly IFpsPlayerMotor _playerMotor;
        private readonly IWaterState _waterState;

        private readonly float _groundCheckInterval;

        private float _groundCheckTimer;
        private bool _groundedCached;

        public bool IsGrounded => _groundedCached;
        public bool IsInWater => _waterState.IsInWater;

        public PlayerEnvironmentState(
            IFpsPlayerMotor playerMotor,
            IWaterState waterState)
        {
            _playerMotor = playerMotor;
            _waterState = waterState;

            _groundCheckInterval = 1f / GroundCheckRate;
        }

        public void Update()
        {
            UpdateGroundCheck();
        }

        private void UpdateGroundCheck()
        {
            _groundCheckTimer -= Time.fixedDeltaTime;

            if (_groundCheckTimer > 0f)
                return;

            _groundCheckTimer = _groundCheckInterval;

            _groundedCached = _playerMotor.IsGroundedCheck();
        }
    }
}