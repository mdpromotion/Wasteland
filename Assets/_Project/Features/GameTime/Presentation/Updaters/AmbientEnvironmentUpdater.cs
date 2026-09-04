using _Project.Features.GameTime.Domain;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Project.Features.GameTime.Presentation.Updaters
{
    public class AmbientEnvironmentUpdater : IGameTimeVisualUpdater
    {
        private DayNightPhase? _lastPhase;

        public AmbientEnvironmentUpdater()
        {
            RenderSettings.ambientMode = AmbientMode.Skybox;
        }

        public void Apply(DayNightPhaseInfo phaseInfo, float rawTime)
        {
            bool isTransition = phaseInfo.IsTransition;

            if (!isTransition && _lastPhase == phaseInfo.Phase)
                return;

            RenderSettings.ambientIntensity = phaseInfo.Phase switch
            {
                DayNightPhase.DayTransition => phaseInfo.T,
                DayNightPhase.NightTransition => 1f - phaseInfo.T,
                DayNightPhase.Day => 1f,
                _ => 0f
            };

            _lastPhase = phaseInfo.Phase;
        }
    }
}