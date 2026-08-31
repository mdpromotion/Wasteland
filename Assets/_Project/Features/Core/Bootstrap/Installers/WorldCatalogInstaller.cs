using _Project.Features.Persistence.Application;
using _Project.Features.Persistence.Infrastructure;
using VContainer;

namespace _Project.Features.Core.Bootstrap.Installers
{
    public static class WorldCatalogInstaller
    {
        public static void Install(IContainerBuilder builder)
        {
            builder.Register<WorldFileStore>(Lifetime.Singleton)
                .As<IWorldReader>()
                .As<IWorldWriter>();

            builder.Register<WorldCatalog>(Lifetime.Singleton)
                .As<IWorldCatalog>();
        }
    }
}