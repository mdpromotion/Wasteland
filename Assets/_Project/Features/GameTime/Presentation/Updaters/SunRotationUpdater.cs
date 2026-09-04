using _Project.Features.GameTime.Domain;
using UnityEngine;

namespace _Project.Features.GameTime.Presentation.Updaters
{
    public class SunRotationUpdater : IGameTimeVisualUpdater
    {
        private readonly Transform _sunTransform;
        private readonly IGameTime _gameTime;
        private readonly float _sunRotationOffset;

        public SunRotationUpdater(Transform sunTransform, IGameTime gameTime, float sunRotationOffset)
        {
            _sunTransform = sunTransform;
            _gameTime = gameTime;
            _sunRotationOffset = sunRotationOffset;
        }

        public void Apply(DayNightPhaseInfo phaseInfo, float rawTime)
        {
            float sunAngle = rawTime / _gameTime.TicksPerDay * 360f + _sunRotationOffset;
            _sunTransform.localRotation = Quaternion.Euler(sunAngle, 0f, 0f);
        }
    }
}