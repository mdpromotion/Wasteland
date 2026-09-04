namespace _Project.Features.GameTime.Domain
{
    public readonly struct DayNightPhaseInfo
    {
        public readonly DayNightPhase Phase;
        public readonly float T;

        public DayNightPhaseInfo(DayNightPhase phase, float t)
        {
            Phase = phase;
            T = t;
        }

        public bool IsTransition => Phase is DayNightPhase.DayTransition or DayNightPhase.NightTransition;
    }

    public enum DayNightPhase { Day, Night, DayTransition, NightTransition }
}