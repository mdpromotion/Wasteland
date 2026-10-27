using _Project.Features.Player.Domain;
using _Project.Features.Player.Infrastructure;
using _Project.Features.Sound.Application;
using _Project.Features.Sound.Domain;
using _Project.Features.Sound.Presentation;
using UnityEngine;
using VContainer;

namespace _Project.Features.Player.Presentation
{
    public sealed class PlayerWaterSoundController : MonoBehaviour
    {
        private PlayerSoundSet _playerSoundSet;
        private IWaterState _waterState;
        private ISoundService _soundService;
        private IPlayerReadOnly _player;

        [Inject]
        public void Construct(
            PlayerSoundSet playerSoundSet,
            IWaterState waterState,
            ISoundService soundService,
            IPlayerReadOnly player)
        {
            _playerSoundSet = playerSoundSet;
            _waterState = waterState;
            _soundService = soundService;
            _player = player;
        }

        private void OnEnable()
        {
            _waterState.OnEnterWater += HandleEnterWater;
            _waterState.OnExitWater += HandleExitWater;
        }

        private void OnDisable()
        {
            _waterState.OnEnterWater -= HandleEnterWater;
            _waterState.OnExitWater -= HandleExitWater;
        }

        private void HandleEnterWater(WaterEnterInfo info)
        {
            PlayWaterSound(_playerSoundSet.WaterEnter);
        }

        private void HandleExitWater()
        {
            PlayWaterSound(_playerSoundSet.WaterExit);
        }

        private void PlayWaterSound(SoundKey key)
        {
            float maxVelocity = _playerSoundSet.WaterSoundMaxVelocity;
            float volumeScale = maxVelocity > 0f
                ? Mathf.Clamp01(_player.Velocity.magnitude / maxVelocity) * 0.5f
                : 0f;

            _soundService.Play(
                key,
                new SoundPlayOptions(_player.Position, volumeScale, false));
        }
    }
}