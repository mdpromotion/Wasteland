using UnityEngine;
using VContainer.Unity;

namespace _Project.Features.Core.Infrastructure
{
    public interface IFPSCounter
    {
        float CurrentFps { get; }
        float InstantFps { get; }
        float AverageFps { get; }
        float PeakFps { get; }
    }

    public sealed class FPSCounter : ITickable, IFPSCounter
    {
        private const float CurrentFpsUpdateInterval = 1f;
        private const float AverageSmoothing = 0.1f;
        private const float PeakRiseSpeed = 0.2f;
        private const float PeakDecaySpeed = 0.01f;
        private const float InitialFps = 60f;

        private int _frameCount;
        private float _timeAccumulator;

        public float CurrentFps { get; private set; }
        public float InstantFps { get; private set; }
        public float AverageFps { get; private set; } = InitialFps;
        public float PeakFps { get; private set; } = InitialFps;

        public void Tick()
        {
            float deltaTime = Time.unscaledDeltaTime;

            if (deltaTime <= 0f)
                return;

            InstantFps = 1f / deltaTime;

            UpdateAverage();
            UpdatePeak();
            UpdateCurrentFps(deltaTime);
        }

        private void UpdateAverage()
        {
            AverageFps = Mathf.Lerp(
                AverageFps,
                InstantFps,
                AverageSmoothing);
        }

        private void UpdatePeak()
        {
            float smoothing = AverageFps > PeakFps
                ? PeakRiseSpeed
                : PeakDecaySpeed;

            PeakFps = Mathf.Lerp(
                PeakFps,
                AverageFps,
                smoothing);
        }

        private void UpdateCurrentFps(float deltaTime)
        {
            _frameCount++;
            _timeAccumulator += deltaTime;

            if (_timeAccumulator < CurrentFpsUpdateInterval)
                return;

            CurrentFps = _frameCount / _timeAccumulator;
            _frameCount = 0;
            _timeAccumulator = 0f;
        }
    }
}