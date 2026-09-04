using _Project.Features.GameTime.Domain;
using UnityEngine;

namespace _Project.Features.GameTime.Infrastructure
{
    public class DayNightPhaseCalculator : IDayNightPhaseCalculator
    {
        private readonly struct DayNightTimings
        {
            public readonly float DayHour;
            public readonly float NightHour;
            public readonly float DayTransitionStart;
            public readonly float NightTransitionStart;

            public DayNightTimings(float dayHour, float nightHour, float dayTransitionStart, float nightTransitionStart)
            {
                DayHour = dayHour;
                NightHour = nightHour;
                DayTransitionStart = dayTransitionStart;
                NightTransitionStart = nightTransitionStart;
            }
        }

        private readonly IGameTime _gameTime;
        private readonly GameTimePresenterSceneConfig _sceneConfig;

        public DayNightPhaseCalculator(IGameTime gameTime, GameTimePresenterSceneConfig sceneConfig)
        {
            _gameTime = gameTime;
            _sceneConfig = sceneConfig;
        }

        public DayNightPhaseInfo Calculate(float time)
        {
            DayNightTimings timings = GetTimings();

            if (IsInTransition(time, timings.DayTransitionStart, timings.DayHour))
            {
                float t = Mathf.InverseLerp(timings.DayTransitionStart, timings.DayHour, time);
                return new DayNightPhaseInfo(DayNightPhase.DayTransition, t);
            }

            if (IsInTransition(time, timings.NightTransitionStart, timings.NightHour))
            {
                float t = Mathf.InverseLerp(timings.NightTransitionStart, timings.NightHour, time);
                return new DayNightPhaseInfo(DayNightPhase.NightTransition, t);
            }

            DayNightPhase phase = IsDay(time, timings.DayHour, timings.NightHour)
                ? DayNightPhase.Day
                : DayNightPhase.Night;

            return new DayNightPhaseInfo(phase, 0f);
        }

        private DayNightTimings GetTimings()
        {
            float transitionDuration = _gameTime.HoursToTicks(_sceneConfig.TransitionDurationHours);
            float dayHour = _gameTime.HoursToTicks(_sceneConfig.DayTransition.Hour);
            float nightHour = _gameTime.HoursToTicks(_sceneConfig.NightTransition.Hour);

            return new DayNightTimings(
                dayHour,
                nightHour,
                dayHour - transitionDuration,
                nightHour - transitionDuration);
        }

        private bool IsInTransition(float time, float start, float end)
        {
            return time >= start && time <= end;
        }

        private bool IsDay(float time, float dayHour, float nightHour)
        {
            return time >= dayHour && time < nightHour;
        }
    }
}