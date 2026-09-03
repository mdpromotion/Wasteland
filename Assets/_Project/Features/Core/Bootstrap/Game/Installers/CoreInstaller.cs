using _Project.Features.Core.Application;
using _Project.Features.Core.Domain;
using _Project.Features.Core.Infrastructure;
using _Project.Features.Cursor.Presentation;
using _Project.Features.Graphics.Presentation;
using VContainer;
using VContainer.Unity;

namespace _Project.Features.Core.Bootstrap.Game.Installers
{
    public static class CoreInstaller
    {
        public static void Install(
            IContainerBuilder builder,
            FrameBudgetConfig frameBudgetConfig)
        {
            builder.Register<GameState>(Lifetime.Singleton)
                .As<IGameStateController>()
                .As<IGameState>();

            builder.RegisterInstance(frameBudgetConfig);

            builder.Register<FrameBudget>(Lifetime.Singleton)
                .As<IFrameBudget>()
                .As<ITickable>();
            
            builder.Register<GameSaveService>(Lifetime.Singleton)
                .As<IGameSaveService>();

            builder.Register<GameLoadService>(Lifetime.Singleton);
            
            builder.Register<WorldAutoSaveSystem>(Lifetime.Singleton)
                .As<ITickable>()
                .AsSelf();

            builder.Register<GameSessionSaveController>(Lifetime.Singleton)
                .As<IGameSessionSaveController>();
            
            builder.Register<CursorLockService>(Lifetime.Singleton)
                .As<ICursorService>();

            builder.Register<CoreTimeService>(Lifetime.Singleton)
                .As<IInitializable>();

            builder.Register<CoreGameLoop>(Lifetime.Singleton)
                .As<IInitializable>();

            builder.RegisterComponentInHierarchy<SceneSettingsPresenter>();
        }
    }
}