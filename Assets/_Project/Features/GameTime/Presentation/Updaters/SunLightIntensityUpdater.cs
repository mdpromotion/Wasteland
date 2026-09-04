using _Project.Features.GameTime.Domain;
using UnityEngine;

namespace _Project.Features.GameTime.Presentation.Updaters
{
    public class SunLightIntensityUpdater : IGameTimeVisualUpdater
    {
        private const float MaximumSunIntensity = 0.5f;
        private const float MinimumSunIntensity = 0.01f;

        private readonly Light _sunLight;
        
        private DayNightPhase? _lastSteadyPhase;

        public SunLightIntensityUpdater(Light sunLight)
        {
            _sunLight = sunLight;
        }

        public void Apply(DayNightPhaseInfo phaseInfo, float rawTime)
        {
            if (!phaseInfo.IsTransition)
            {
                if (_lastSteadyPhase == phaseInfo.Phase)
                    return;

                _sunLight.intensity = phaseInfo.Phase == DayNightPhase.Day
                    ? MaximumSunIntensity
                    : MinimumSunIntensity;

                _lastSteadyPhase = phaseInfo.Phase;
                return;
            }

            _lastSteadyPhase = null;
            _sunLight.intensity = phaseInfo.Phase == DayNightPhase.DayTransition
                ? Mathf.Lerp(MinimumSunIntensity, MaximumSunIntensity, phaseInfo.T)
                : Mathf.Lerp(MaximumSunIntensity, MinimumSunIntensity, phaseInfo.T);
        }
    }
}