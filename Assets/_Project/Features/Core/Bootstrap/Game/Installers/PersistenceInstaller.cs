using _Project.Features.Core.Persistence.Regions;
using _Project.Features.ProceduralWorld.Application.Persistence;
using _Project.Features.ProceduralWorld.Domain.Persistence;
using _Project.Features.ProceduralWorld.Infrastructure;
using _Project.Features.ProceduralWorld.Infrastructure.Chunks;
using VContainer;
using VContainer.Unity;

namespace _Project.Features.Core.Bootstrap.Game.Installers
{
    public static class PersistenceInstaller
    {
        public static void Install(IContainerBuilder builder)
        {
            builder.Register<PalRegionFileStore>(Lifetime.Singleton)
                .As<IPalRegionReader>()
                .As<IPalRegionWriter>();

            builder.Register<ChunkDeltaSerializer>(Lifetime.Singleton);

            builder.Register<ChunkDeltaStore>(Lifetime.Singleton);

            builder.RegisterInstance(
                new GeneratorVersionStamp(
                    vegetationVersion: 1));

            builder.Register<DirtyChunkRegistry>(Lifetime.Singleton)
                .As<IDirtyChunkRegistry>();

            builder.Register<ChunkMutationTracker>(Lifetime.Singleton)
                .As<IChunkMutationTracker>();

            builder.Register<WorldSaveService>(Lifetime.Singleton)
                .As<IWorldSaveService>();

            builder.Register<WorldAutoSaveSystem>(Lifetime.Singleton)
                .As<ITickable>()
                .AsSelf();

            builder.Register<DeltaApplicationStage>(Lifetime.Singleton)
                .As<IDeltaStage>();
        }
    }
}