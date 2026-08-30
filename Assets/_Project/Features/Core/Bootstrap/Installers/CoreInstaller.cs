using _Project.Features.Core.Infrastructure;
using _Project.Features.UI.Application;
using _Project.Features.UI.Infrastructure;
using _Project.Features.UI.Menus.LoadingScreen.View;
using VContainer;
using VContainer.Unity;

namespace _Project.Features.Core.Bootstrap.Installers
{
    public static class CoreInstaller
    {
        public static void Install(
            IContainerBuilder builder,
            SceneDatabase sceneDatabase,
            LoadingScreenView loadingScreenView)
        {
            builder.RegisterInstance(loadingScreenView);
            builder.RegisterInstance(sceneDatabase);

            builder.Register<ILoadSceneService, LoadSceneService>(Lifetime.Singleton);

            builder.Register<BootstrapEntryPoint>(Lifetime.Singleton)
                .As<IInitializable>();

            builder.Register<SceneTransitionService>(Lifetime.Singleton);
            builder.Register<LoadSceneController>(Lifetime.Singleton);
        }
    }
}