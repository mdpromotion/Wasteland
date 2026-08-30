using _Project.Features.Persistence.Infrastructure;
using VContainer;

namespace _Project.Features.Core.Bootstrap.Installers
{
    public static class JsonPersistenceInstaller
    {
        public static void Install(IContainerBuilder builder)
        {
            builder.Register<JsonFileStore>(Lifetime.Singleton)
                .As<IJsonReader>()
                .As<IJsonWriter>();
        }
    }
}