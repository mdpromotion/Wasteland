namespace _Project.Features.GameTime.Domain
{
    public interface IDayNightPhaseCalculator
    {
        DayNightPhaseInfo Calculate(float time);
    }
}