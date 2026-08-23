using System;
using UnityEngine;
using VContainer;
using _Project.Features.Player.Domain;
using _Project.Features.ProceduralWorld.Domain.Hydrology;
using _Project.Features.ProceduralWorld.Infrastructure.Hydrology;

namespace _Project.Features.Player.Infrastructure
{
    public readonly struct WaterEnterInfo
    {
        public readonly float SurfaceHeight;
        public readonly float Mask;

        public WaterEnterInfo(float surfaceHeight, float mask)
        {
            SurfaceHeight = surfaceHeight;
            Mask = mask;
        }
    }

    public sealed class WaterVolumeTracker : MonoBehaviour, IWaterState
    {
        [SerializeField] private float _enterMaskThreshold = 0.1f;
        [SerializeField] private float _exitMaskThreshold = 0.03f;

        private IPlayerReadOnly _player;
        private IWaterQuery _waterQuery;

        private bool _isInWater;

        public event Action<WaterEnterInfo> OnEnterWater;
        public event Action OnExitWater;

        public bool IsInWater => _isInWater;

        [Inject]
        public void Construct(
            IPlayerReadOnly player,
            IWaterQuery waterQuery)
        {
            _player = player;
            _waterQuery = waterQuery;
        }

        private void Update()
        {
            Vector3 position = _player.Position;

            bool hasSample = _waterQuery.TryGetWaterState(
                position,
                out WaterSample sample);

            if (!hasSample)
            {
                if (_isInWater)
                {
                    _isInWater = false;
                    OnExitWater?.Invoke();
                }

                return;
            }

            float threshold = _isInWater
                ? _exitMaskThreshold
                : _enterMaskThreshold;

            bool submerged = sample.IsSubmerged(position.y, threshold);

            if (submerged && !_isInWater)
            {
                _isInWater = true;
                OnEnterWater?.Invoke(
                    new WaterEnterInfo(sample.WorldSurfaceHeight, sample.Mask));
            }
            else if (!submerged && _isInWater)
            {
                _isInWater = false;
                OnExitWater?.Invoke();
            }
        }
    }
}