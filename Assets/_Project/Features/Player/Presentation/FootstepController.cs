using System.Diagnostics;
using _Project.Features.Camera.Infrastructure;
using _Project.Features.Core;
using _Project.Features.Core.Presentation;
using _Project.Features.Player.Application;
using _Project.Features.Player.Domain;
using _Project.Features.Player.Infrastructure;
using _Project.Features.Sound.Domain;
using _Project.Features.Sound.Presentation;
using UnityEngine;
using VContainer;

namespace _Project.Features.Player.Presentation
{
    public sealed class FootstepController : MonoBehaviour
    {
        [SerializeField] private PlayerSoundSet _soundSet;

        [SerializeField] private float _walkStepDistance = 2.2f;
        [SerializeField] private float _sprintStepDistance = 3f;
        [SerializeField] private float _crouchStepDistance = 1.4f;
        [SerializeField] private float _minSpeedForStep = 0.5f;

        private IPlayerReadOnly _player;
        private IPlayerController _controller;
        private IPlayerEnvironmentState _environmentState;
        private IPlayerInputReader _input;
        private IPlayerStanceState _stance;
        private ISoundService _soundService;

        private readonly NonRepeatingRandomKeyPicker _stepPicker = new();

        private float _distanceAccumulator;

        [Inject]
        public void Construct(
            IPlayerReadOnly player,
            IPlayerController controller,
            IPlayerEnvironmentState environmentState,
            IPlayerInputReader input,
            IPlayerStanceState stance,
            ISoundService soundService)
        {
            _player = player;
            _controller = controller;
            _environmentState = environmentState;
            _input = input;
            _stance = stance;
            _soundService = soundService;
        }

        private void OnEnable()
        {
            if (_controller == null)
                return;

            _controller.OnJumped += HandleJumped;
            _controller.OnLanded += HandleLanded;
        }

        private void OnDisable()
        {
            if (_controller == null)
                return;

            _controller.OnJumped -= HandleJumped;
            _controller.OnLanded -= HandleLanded;
        }

        private void FixedUpdate()
        {
            if (_player == null || _controller == null || _input == null || _soundService == null)
                return;

            bool clearGroundStep = _environmentState.IsGrounded && !_environmentState.IsInWater;

            if (!clearGroundStep)
            {
                _distanceAccumulator = 0f;
                return;
            }

            Vector3 velocity = _player.Velocity;
            float planarSpeed = new Vector2(velocity.x, velocity.z).magnitude;

            if (planarSpeed < _minSpeedForStep)
            {
                _distanceAccumulator = 0f;
                return;
            }

            bool crouching = _stance != null && _stance.IsCrouching;
            bool sprinting = _input.SprintPressed && !crouching;

            float stepDistance = crouching
                ? _crouchStepDistance
                : sprinting
                    ? _sprintStepDistance
                    : _walkStepDistance;

            _distanceAccumulator += planarSpeed * Time.fixedDeltaTime;

            if (_distanceAccumulator >= stepDistance)
            {
                _distanceAccumulator -= stepDistance;
                PlayStep(sprinting, crouching);
            }
        }

        private void PlayStep(bool sprinting, bool crouching)
        {
            SoundKey[] pool = sprinting
                ? _soundSet.RunFootsteps
                : _soundSet.WalkFootsteps;

            if (pool == null || pool.Length == 0)
                return;

            SoundKey key = _stepPicker.Pick(pool);

            _soundService.Play(key);
        }

        private void HandleJumped()
        {
            SoundKey[] pool = _soundSet.JumpStart;
            if (pool == null || pool.Length == 0)
                return;
            
            SoundKey key = _stepPicker.Pick(pool);
            
            _soundService.Play(key);
        }

        private void HandleLanded()
        {
            SoundKey[] pool = _soundSet.JumpLand;
            if (pool == null || pool.Length == 0)
                return;
            
            SoundKey key = _stepPicker.Pick(pool);
            
            _soundService.Play(key);
        }
    }
}