using _Project.Features.Core.Infrastructure;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace _Project.Tests.Core
{
    public sealed class FrameBudgetTests
    {
        private FrameBudget _budget;
        private FrameBudgetConfig _config;
        private FakeFPSCounter _fpsCounter;

        [SetUp]
        public void SetUp()
        {
            _config = CreateConfig();
            _fpsCounter = new FakeFPSCounter
            {
                InstantFps = 60f,
                AverageFps = 60f,
                PeakFps = 60f
            };

            _budget = new FrameBudget(_config, _fpsCounter);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void TryBeginOperation_AllowsMaximumOperationsPerFrame()
        {
            _budget.Tick();

            for (int i = 0; i < _config.MaxOperationsPerFrame; i++)
            {
                bool result = _budget.TryBeginOperation(
                    out IFrameBudgetOperation operation);

                Assert.That(result, Is.True);
                Assert.That(operation, Is.Not.Null);
            }
        }

        [Test]
        public void TryBeginOperation_DoesNotAllowMoreThanMaximumOperationsPerFrame()
        {
            _budget.Tick();

            for (int i = 0; i < _config.MaxOperationsPerFrame; i++)
                _budget.TryBeginOperation(out _);

            bool result = _budget.TryBeginOperation(
                out IFrameBudgetOperation operation);

            Assert.That(result, Is.False);
            Assert.That(operation, Is.Null);
        }

        [Test]
        public void Tick_ResetsSpentOperations()
        {
            _budget.Tick();

            for (int i = 0; i < _config.MaxOperationsPerFrame; i++)
                _budget.TryBeginOperation(out _);

            Assert.That(
                _budget.TryBeginOperation(out _),
                Is.False);

            _budget.Tick();

            Assert.That(
                _budget.TryBeginOperation(out IFrameBudgetOperation operation),
                Is.True);

            Assert.That(operation, Is.Not.Null);
        }

        [Test]
        public void TryBeginOperation_ReturnsSameOperationInstance()
        {
            _budget.Tick();

            _budget.TryBeginOperation(
                out IFrameBudgetOperation first);

            _budget.TryBeginOperation(
                out IFrameBudgetOperation second);

            Assert.That(first, Is.SameAs(second));
        }

        [Test]
        public void Tick_UsesMinimumOperations_WhenLagSpikeDetected()
        {
            _fpsCounter.InstantFps = 20f;
            _fpsCounter.AverageFps = 60f;

            _budget.Tick();

            for (int i = 0; i < _config.MinOperationsPerFrame; i++)
            {
                Assert.That(
                    _budget.TryBeginOperation(out _),
                    Is.True);
            }

            Assert.That(
                _budget.TryBeginOperation(out _),
                Is.False);
        }

        [Test]
        public void Tick_UsesMaximumOperations_AtHighFps()
        {
            _fpsCounter.InstantFps = 60f;
            _fpsCounter.AverageFps = 60f;
            _fpsCounter.PeakFps = 60f;

            _budget.Tick();

            int successfulOperations = 0;

            while (_budget.TryBeginOperation(out _))
                successfulOperations++;

            Assert.That(
                successfulOperations,
                Is.EqualTo(_config.MaxOperationsPerFrame));
        }

        [Test]
        public void Tick_UsesMinimumOperations_AtLowFps()
        {
            _fpsCounter.InstantFps = 30f;
            _fpsCounter.AverageFps = 30f;
            _fpsCounter.PeakFps = 30f;

            _budget.Tick();

            int successfulOperations = 0;

            while (_budget.TryBeginOperation(out _))
                successfulOperations++;

            Assert.That(
                successfulOperations,
                Is.EqualTo(_config.MinOperationsPerFrame));
        }

        private static FrameBudgetConfig CreateConfig()
        {
            FrameBudgetConfig config =
                ScriptableObject.CreateInstance<FrameBudgetConfig>();

            SerializedObject serializedConfig =
                new SerializedObject(config);

            serializedConfig.FindProperty("lowFpsThreshold").floatValue = 30f;
            serializedConfig.FindProperty("highFpsThreshold").floatValue = 60f;
            serializedConfig.FindProperty("minOperationsPerFrame").intValue = 0;
            serializedConfig.FindProperty("maxOperationsPerFrame").intValue = 4;
            serializedConfig.FindProperty("fpsSmoothingDown").floatValue = 1f;
            serializedConfig.FindProperty("fpsSmoothingUp").floatValue = 0.05f;
            serializedConfig.FindProperty("lagDropRatio").floatValue = 0.6f;

            serializedConfig.ApplyModifiedPropertiesWithoutUndo();

            return config;
        }

        private sealed class FakeFPSCounter : IFPSCounter
        {
            public float CurrentFps { get; set; }
            public float InstantFps { get; set; }
            public float AverageFps { get; set; }
            public float PeakFps { get; set; }
        }
    }
}