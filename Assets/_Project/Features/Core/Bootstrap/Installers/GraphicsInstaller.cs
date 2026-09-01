using _Project.Features.Graphics.Domain;
using _Project.Features.Graphics.Infrastucture;
using _Project.Features.UI.Application;
using _Project.Features.UI.Infrastructure;
using VContainer;
using VContainer.Unity;

namespace _Project.Features.Core.Bootstrap.Installers
{
    public static class GraphicsInstaller
    {
        public static void Install(
            IContainerBuilder builder,
            GraphicsQualityConfig qualityConfig,
            FogConfig fogConfig,
            GraphicsConfigResolver graphicsConfigResolver)
        {
            builder.RegisterInstance(qualityConfig);
            builder.RegisterInstance(fogConfig);

            builder.Register<FogState>(Lifetime.Singleton);
            builder.Register<GraphicsState>(Lifetime.Singleton);

            builder.Register<FogSettings>(Lifetime.Singleton)
                .As<IFogSettings>()
                .As<IInitializable>();

            builder.RegisterInstance(graphicsConfigResolver)
                .As<IGraphicsConfigResolver>()
                .AsSelf();

            builder.RegisterBuildCallback(container =>
            {
                container.Inject(graphicsConfigResolver);
            });

            builder.Register<GraphicsSettingsRepository>(Lifetime.Singleton)
                .As<IGraphicsSettingsRepository>();

            builder.Register<UnitySettingsApplier>(Lifetime.Singleton)
                .As<IInitializable>();

            builder.Register<FogApplier>(Lifetime.Singleton)
                .As<IFogApplier>();

            builder.Register<FogAnimator>(Lifetime.Singleton)
                .As<IFogAnimator>();
        }
    }
}