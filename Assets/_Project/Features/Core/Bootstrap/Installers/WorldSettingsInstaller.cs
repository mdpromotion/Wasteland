using _Project.Features.ProceduralWorld.Domain.World;
using VContainer;

namespace _Project.Features.Core.Bootstrap.Installers
{
    public static class WorldSettingsInstaller
    {
        public static void Install(IContainerBuilder builder, WorldSettingsConfig worldSettingsConfig)
        {
            builder.RegisterInstance(worldSettingsConfig);

            builder.Register<WorldSettings>(Lifetime.Singleton)
                .As<IWorldSettings>()
                .As<IWorldSettingsController>();
        }
    }
}