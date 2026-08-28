using _Project.Features.GameTime.Application;
using _Project.Features.GameTime.Domain;
using _Project.Features.GameTime.Presentation;
using VContainer;
using VContainer.Unity;

namespace _Project.Features.Core.Bootstrap.Game.Installers
{
    public static class GameTimeInstaller
    {
        public static void Install(IContainerBuilder builder)
        {
            builder.Register<GameTime.Domain.GameTime>(Lifetime.Singleton)
                .As<IGameTime>()
                .AsSelf();

            builder.Register<GameTimeController>(Lifetime.Singleton)
                .As<IInitializable>();

            builder.RegisterComponentInHierarchy<GameTimePresenter>();
        }
    }
}