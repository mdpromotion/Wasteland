using _Project.Features.Shared.Application;
using _Project.Features.Tick;
using _Project.Features.Tick.Application;
using _Project.Features.Tick.Domain;
using VContainer;
using VContainer.Unity;

namespace _Project.Features.Core.Bootstrap.Game.Installers
{
    public static class TickInstaller
    {
        public static void Install(IContainerBuilder builder)
        {
            builder.Register<TickData>(Lifetime.Singleton);

            builder.Register<TickController>(Lifetime.Singleton)
                .As<IFixedTickable>()
                .As<ITick>();

            builder.Register<TickDebug>(Lifetime.Singleton)
                .As<IInitializable>()
                .AsSelf();
        }
    }
}