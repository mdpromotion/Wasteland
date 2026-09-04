using System;
using _Project.Features.GameTime.Domain;
using _Project.Features.GameTime.Infrastructure;
using NUnit.Framework;
using UnityEngine;

namespace _Project.Tests.GameTime
{
    public class DayNightPhaseCalculatorTests
    {
        private class FakeGameTime : IGameTime
        {
            public float CurrentTime { get; set; }
            public float TicksPerDay => 24f;

            public event Action<float> TimeChanged;

            public float HoursToTicks(float hours) => hours;

            public void RaiseTimeChanged(float time) => TimeChanged?.Invoke(time);
        }

        private FakeGameTime _gameTime;
        private GameTimePresenterSceneConfig _sceneConfig;
        private DayNightPhaseCalculator _calculator;

        // Конфиг: день начинается в 6:00, ночь начинается в 20:00,
        // переход занимает 2 часа (значит DayTransition: 4:00-6:00, NightTransition: 18:00-20:00).
        private const float DayHour = 6f;
        private const float NightHour = 20f;
        private const float TransitionDurationHours = 2f;

        [SetUp]
        public void SetUp()
        {
            _gameTime = new FakeGameTime();

            _sceneConfig = ScriptableObject.CreateInstance<GameTimePresenterSceneConfig>();
            _sceneConfig.SetForTest(
                sunRotationOffset: 0f,
                transitionDurationHours: TransitionDurationHours,
                dayHour: DayHour,
                nightHour: NightHour);

            _calculator = new DayNightPhaseCalculator(_gameTime, _sceneConfig);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_sceneConfig);
        }

        [Test]
        public void Calculate_AtNoon_ReturnsDay()
        {
            DayNightPhaseInfo info = _calculator.Calculate(12f);

            Assert.AreEqual(DayNightPhase.Day, info.Phase);
            Assert.IsFalse(info.IsTransition);
        }

        [Test]
        public void Calculate_AtMidnight_ReturnsNight()
        {
            DayNightPhaseInfo info = _calculator.Calculate(0f);

            Assert.AreEqual(DayNightPhase.Night, info.Phase);
            Assert.IsFalse(info.IsTransition);
        }

        [Test]
        public void Calculate_JustBeforeDayHour_ReturnsNight()
        {
            // DayTransition начинается в 4:00, значит 3:59 — ещё чистая ночь
            DayNightPhaseInfo info = _calculator.Calculate(3.99f);

            Assert.AreEqual(DayNightPhase.Night, info.Phase);
        }

        [Test]
        public void Calculate_AtDayTransitionStart_ReturnsDayTransitionWithZeroT()
        {
            DayNightPhaseInfo info = _calculator.Calculate(4f);

            Assert.AreEqual(DayNightPhase.DayTransition, info.Phase);
            Assert.IsTrue(info.IsTransition);
            Assert.AreEqual(0f, info.T, 0.0001f);
        }

        [Test]
        public void Calculate_AtDayTransitionMidpoint_ReturnsHalfT()
        {
            DayNightPhaseInfo info = _calculator.Calculate(5f); // середина между 4:00 и 6:00

            Assert.AreEqual(DayNightPhase.DayTransition, info.Phase);
            Assert.AreEqual(0.5f, info.T, 0.0001f);
        }

        [Test]
        public void Calculate_AtDayHour_ReturnsDayTransitionWithFullT()
        {
            // Границы включительные (>=  и <=), поэтому 6:00 — ещё DayTransition с T=1,
            // а не Day. Это осознанное поведение текущей реализации — фиксируем его тестом,
            // чтобы случайное изменение границ не прошло незамеченным.
            DayNightPhaseInfo info = _calculator.Calculate(6f);

            Assert.AreEqual(DayNightPhase.DayTransition, info.Phase);
            Assert.AreEqual(1f, info.T, 0.0001f);
        }

        [Test]
        public void Calculate_JustAfterDayHour_ReturnsDay()
        {
            DayNightPhaseInfo info = _calculator.Calculate(6.01f);

            Assert.AreEqual(DayNightPhase.Day, info.Phase);
        }

        [Test]
        public void Calculate_AtNightTransitionStart_ReturnsNightTransitionWithZeroT()
        {
            DayNightPhaseInfo info = _calculator.Calculate(18f);

            Assert.AreEqual(DayNightPhase.NightTransition, info.Phase);
            Assert.AreEqual(0f, info.T, 0.0001f);
        }

        [Test]
        public void Calculate_AtNightTransitionMidpoint_ReturnsHalfT()
        {
            DayNightPhaseInfo info = _calculator.Calculate(19f);

            Assert.AreEqual(DayNightPhase.NightTransition, info.Phase);
            Assert.AreEqual(0.5f, info.T, 0.0001f);
        }

        [Test]
        public void Calculate_AtNightHour_ReturnsNightTransitionWithFullT()
        {
            DayNightPhaseInfo info = _calculator.Calculate(20f);

            Assert.AreEqual(DayNightPhase.NightTransition, info.Phase);
            Assert.AreEqual(1f, info.T, 0.0001f);
        }

        [Test]
        public void Calculate_JustAfterNightHour_ReturnsNight()
        {
            DayNightPhaseInfo info = _calculator.Calculate(20.01f);

            Assert.AreEqual(DayNightPhase.Night, info.Phase);
        }

        [Test]
        public void Calculate_FullDayCycle_NeverThrowsAndAlwaysReturnsValidPhase()
        {
            // Sanity-проход по всем суткам с мелким шагом — защищает от NaN/exceptions
            // на границах, если кто-то поменяет TicksPerDay или конфиг в будущем.
            for (float t = 0f; t < 24f; t += 0.1f)
            {
                DayNightPhaseInfo info = _calculator.Calculate(t);

                Assert.IsTrue(Enum.IsDefined(typeof(DayNightPhase), info.Phase));
                Assert.IsFalse(float.IsNaN(info.T));
                Assert.GreaterOrEqual(info.T, 0f);
                Assert.LessOrEqual(info.T, 1f);
            }
        }
    }
}