using _Project.Features.GameTime.Domain;
using _Project.Features.GameTime.Presentation.Updaters;
using UnityEngine;
using UnityEngine.Rendering;

public class AmbientEnvironmentUpdater : IGameTimeVisualUpdater
{
    private const float NightAmbientIntensity = 0.5f;
    private const float DayAmbientIntensity = 1f;

    public AmbientEnvironmentUpdater()
    {
        RenderSettings.ambientMode = AmbientMode.Skybox;
    }

    public void Apply(DayNightPhaseInfo phaseInfo, float rawTime)
    {
        RenderSettings.ambientIntensity = phaseInfo.Phase switch
        {
            DayNightPhase.DayTransition => Mathf.Lerp(NightAmbientIntensity, DayAmbientIntensity, phaseInfo.T),
            DayNightPhase.NightTransition => Mathf.Lerp(DayAmbientIntensity, NightAmbientIntensity, phaseInfo.T),
            DayNightPhase.Day => DayAmbientIntensity,
            _ => NightAmbientIntensity
        };
    }
}