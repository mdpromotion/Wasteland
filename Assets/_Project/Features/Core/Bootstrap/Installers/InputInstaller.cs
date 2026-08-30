using _Project.Features.Core.Presentation;
using VContainer;
using VContainer.Unity;

namespace _Project.Features.Core.Bootstrap.Installers
{
    public static class InputInstaller
    {
        public static void Install(IContainerBuilder builder)
        {
            builder.Register<InputSystem_Actions>(Lifetime.Singleton);

            builder.Register<InputReader>(Lifetime.Singleton)
                .As<IPlayerInputReader>()
                .As<IPlayerUIInputReader>()
                .As<IInitializable>();
        }
    }
}