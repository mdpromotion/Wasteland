using System;

namespace _Project.Features.GameTime.Domain
{
    public class GameTime : IGameTime
    {
        private const float TickPerDay = 24_000f;
        
        public float CurrentTime { get; private set; }
        public float TicksPerDay => TickPerDay;

        public event Action<float> TimeChanged;

        public float HoursToTicks(float hours)
        {
            return hours / 24f * TicksPerDay;
        }

        public void Advance(float delta = 1f)
        {
            CurrentTime += delta;

            if (CurrentTime >= TickPerDay)
            {
                CurrentTime = 0;
            }

            TimeChanged?.Invoke(CurrentTime);
        }

        public float GetDefaultTime()
            => TickPerDay / 4;

        public void SetTime(float time)
        {
            if (time is < 0 or > TickPerDay)
                return;

            CurrentTime = time;

            TimeChanged?.Invoke(CurrentTime);
        }
    }
}