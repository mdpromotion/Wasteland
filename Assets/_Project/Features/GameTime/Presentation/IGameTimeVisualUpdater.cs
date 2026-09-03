using _Project.Features.GameTime.Domain;

namespace _Project.Features.GameTime.Presentation
{
    public interface IGameTimeVisualUpdater
    {
        void Apply(DayNightPhaseInfo phaseInfo, float rawTime);
    }
}