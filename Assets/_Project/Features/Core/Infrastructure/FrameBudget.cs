using System;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Features.Core.Infrastructure
{
    public interface IFrameBudget
    {
        bool TryBeginOperation(out IFrameBudgetOperation operation);
    }

    public interface IFrameBudgetOperation : IDisposable
    {
    }

    public sealed class FrameBudget : IFrameBudget, ITickable
    {
        private static readonly IFrameBudgetOperation NoOpOperation = new Operation();

        private readonly FrameBudgetConfig _config;
        private readonly IFPSCounter _fpsCounter;

        private float _smoothedFps;
        private int _allowedOperations;
        private int _spentOperations;

        public FrameBudget(
            FrameBudgetConfig config,
            IFPSCounter fpsCounter)
        {
            _config = config;
            _fpsCounter = fpsCounter;

            _smoothedFps = _config.HighFpsThreshold;
            _allowedOperations = _config.MaxOperationsPerFrame;
        }

        public void Tick()
        {
            UpdateSmoothedFps();

            _spentOperations = 0;
            _allowedOperations = CalculateAllowedOperations();
        }

        public bool TryBeginOperation(out IFrameBudgetOperation operation)
        {
            if (_spentOperations >= _allowedOperations)
            {
                operation = null;
                return false;
            }

            _spentOperations++;
            operation = NoOpOperation;

            return true;
        }

        private void UpdateSmoothedFps()
        {
            float rawFps = _fpsCounter.InstantFps;

            if (rawFps <= 0f)
                return;

            float smoothing = rawFps < _smoothedFps
                ? _config.FpsSmoothingDown
                : _config.FpsSmoothingUp;

            _smoothedFps = Mathf.Lerp(
                _smoothedFps,
                rawFps,
                smoothing);
        }

        private bool IsLagSpike()
        {
            float average = _fpsCounter.AverageFps;

            if (average <= 0f)
                return false;

            float threshold = average * _config.LagDropRatio;

            return _fpsCounter.InstantFps < threshold;
        }

        private int CalculateAllowedOperations()
        {
            if (IsLagSpike())
                return _config.MinOperationsPerFrame;

            float lowFps = _config.LowFpsThreshold;

            float highFps = Mathf.Max(
                _config.HighFpsThreshold,
                _fpsCounter.PeakFps);

            float t = Mathf.InverseLerp(
                lowFps,
                highFps,
                _smoothedFps);

            float operations = Mathf.Lerp(
                _config.MinOperationsPerFrame,
                _config.MaxOperationsPerFrame,
                t);

            return Mathf.Clamp(
                Mathf.RoundToInt(operations),
                _config.MinOperationsPerFrame,
                _config.MaxOperationsPerFrame);
        }

        private sealed class Operation : IFrameBudgetOperation
        {
            public void Dispose()
            {
            }
        }
    }
}