using _Project.Features.GameTime.Domain;
using _Project.Features.GameTime.Infrastructure;
using _Project.Features.Graphics.Domain;
using _Project.Features.Graphics.Infrastucture;

namespace _Project.Features.GameTime.Presentation.Updaters
{
    public class FogTargetUpdater : IGameTimeVisualUpdater
    {
        private const float NightFogStartDistance = 0f;
        private const float NightFogEndDistance = 300f;

        private readonly IFogSettings _fogSettings;
        private readonly IFogAnimator _fogAnimator;
        private readonly GameTimePresenterSceneConfig _sceneConfig;
        
        private DayNightPhase? _lastSteadyPhase;

        public FogTargetUpdater(IFogSettings fogSettings, IFogAnimator fogAnimator, GameTimePresenterSceneConfig sceneConfig)
        {
            _fogSettings = fogSettings;
            _fogAnimator = fogAnimator;
            _sceneConfig = sceneConfig;
        }

        public void Apply(DayNightPhaseInfo phaseInfo, float rawTime)
        {
            if (!phaseInfo.IsTransition)
            {
                if (_lastSteadyPhase == phaseInfo.Phase)
                    return;

                _fogAnimator.SetTarget(phaseInfo.Phase == DayNightPhase.Day ? GetDayFogState() : GetNightFogState());
                _lastSteadyPhase = phaseInfo.Phase;
                return;
            }

            _lastSteadyPhase = null;

            FogState target = phaseInfo.Phase == DayNightPhase.DayTransition
                ? FogState.Lerp(GetNightFogState(), GetDayFogState(), phaseInfo.T)
                : FogState.Lerp(GetDayFogState(), GetNightFogState(), phaseInfo.T);

            _fogAnimator.SetTarget(target);
        }

        private FogState GetDayFogState()
        {
            return new FogState(
                _sceneConfig.DayTransition.FogColor,
                _fogSettings.OriginalFogStartDistance,
                _fogSettings.OriginalFogEndDistance);
        }

        private FogState GetNightFogState()
        {
            return new FogState(
                _sceneConfig.NightTransition.FogColor,
                NightFogStartDistance,
                NightFogEndDistance);
        }
    }
}