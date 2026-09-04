using _Project.Features.GameTime.Domain;
using _Project.Features.GameTime.Infrastructure;
using _Project.Features.GameTime.Presentation.Updaters;
using _Project.Features.Graphics.Domain;
using _Project.Features.Graphics.Infrastucture;
using UnityEngine;
using VContainer;

namespace _Project.Features.GameTime.Presentation
{
    public class GameTimePresenter : MonoBehaviour
    {
        [SerializeField] private Transform sunTransform;
        [SerializeField] private GameTimePresenterSceneConfig sceneConfig;

        private IGameTime _gameTime;
        private IFogSettings _fogSettings;
        private IFogAnimator _fogAnimator;

        private IDayNightPhaseCalculator _phaseCalculator;
        private IGameTimeVisualUpdater[] _updaters;

        private readonly object _timeLock = new object();
        private float _pendingTime;
        private bool _hasPendingTime;

        [Inject]
        public void Construct(
            IGameTime gameTime,
            IFogSettings fogSettings,
            IFogAnimator fogAnimator)
        {
            _gameTime = gameTime;
            _fogSettings = fogSettings;
            _fogAnimator = fogAnimator;
        }

        private void Start()
        {
            Light sunLight = sunTransform.GetComponent<Light>();
            if (!sunLight)
                sunLight = sunTransform.gameObject.AddComponent<Light>();

            _phaseCalculator = new DayNightPhaseCalculator(_gameTime, sceneConfig);

            _updaters = new IGameTimeVisualUpdater[]
            {
                new SunRotationUpdater(sunTransform, _gameTime, sceneConfig.SunRotationOffset),
                new SunLightIntensityUpdater(sunLight),
                new AmbientEnvironmentUpdater(),
                new FogTargetUpdater(_fogSettings, _fogAnimator, sceneConfig),
                new GlobalShaderTimeUpdater(_gameTime)
            };

            _gameTime.TimeChanged += OnTimeChanged;

            ApplyTime(_gameTime.CurrentTime);
        }

        private void Update()
        {
            float? time = ConsumePendingTime();
            if (time.HasValue)
                ApplyTime(time.Value);

            _fogAnimator.Tick(Time.deltaTime);
        }

        private void ApplyTime(float time)
        {
            DayNightPhaseInfo phaseInfo = _phaseCalculator.Calculate(time);

            foreach (IGameTimeVisualUpdater updater in _updaters)
                updater.Apply(phaseInfo, time);
        }

        private float? ConsumePendingTime()
        {
            lock (_timeLock)
            {
                if (!_hasPendingTime)
                    return null;

                _hasPendingTime = false;
                return _pendingTime;
            }
        }

        private void OnTimeChanged(float time)
        {
            lock (_timeLock)
            {
                _pendingTime = time;
                _hasPendingTime = true;
            }
        }

        private void OnDestroy()
        {
            if (_gameTime != null)
                _gameTime.TimeChanged -= OnTimeChanged;
        }
    }
}