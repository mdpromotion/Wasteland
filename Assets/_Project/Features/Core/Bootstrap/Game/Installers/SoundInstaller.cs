using _Project.Features.Sound.Application;
using _Project.Features.Sound.Infrastructure;
using _Project.Features.Sound.Presentation;
using VContainer;
using VContainer.Unity;

namespace _Project.Features.Core.Bootstrap.Game.Installers
{
    public static class SoundInstaller
    {
        public static void Install(
            IContainerBuilder builder,
            SoundDatabase database,
            int globalMaxVoices)
        {
            builder.RegisterInstance(database);

            builder.Register(
                _ => new SoundPlaybackGuard(globalMaxVoices),
                Lifetime.Singleton);

            builder.RegisterComponentOnNewGameObject<SoundVoicePool>(
                Lifetime.Singleton,
                "SoundVoicePool");

            builder.Register<SoundService>(Lifetime.Singleton)
                .As<ISoundService>();
        }
    }
}