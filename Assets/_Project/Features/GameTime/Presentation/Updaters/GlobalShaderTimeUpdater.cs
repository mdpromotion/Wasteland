using _Project.Features.GameTime.Domain;
using UnityEngine;

namespace _Project.Features.GameTime.Presentation.Updaters
{
    public class GlobalShaderTimeUpdater : IGameTimeVisualUpdater
    {
        private static readonly int GlobalTimeOfDayId = Shader.PropertyToID("_GlobalTimeOfDay");

        private readonly IGameTime _gameTime;

        public GlobalShaderTimeUpdater(IGameTime gameTime)
        {
            _gameTime = gameTime;
        }

        public void Apply(DayNightPhaseInfo phaseInfo, float rawTime)
        {
            float normalizedTime = rawTime / _gameTime.TicksPerDay;
            Shader.SetGlobalFloat(GlobalTimeOfDayId, normalizedTime);
        }
    }
}