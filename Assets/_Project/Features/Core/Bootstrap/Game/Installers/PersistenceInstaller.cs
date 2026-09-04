using _Project.Features.Persistence.Application;
using _Project.Features.Persistence.Domain;
using _Project.Features.Persistence.Infrastructure;
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

            builder.Register<ChunkDeltaStore>(Lifetime.Singleton)
                .As<IChunkDeltaStore>();

            builder.RegisterInstance(
                new GeneratorVersionStamp(
                    vegetationVersion: 1));

            builder.Register<DirtyChunkRegistry>(Lifetime.Singleton)
                .As<IDirtyChunkRegistry>();

            builder.Register<ChunkMutationTracker>(Lifetime.Singleton)
                .As<IChunkMutationTracker>();

            builder.Register<WorldSaveService>(Lifetime.Singleton)
                .As<IWorldSaveService>();
            
            builder.Register<PlayerFileStore>(Lifetime.Singleton)
                .As<IPlayerSaveReader>()
                .As<IPlayerSaveWriter>();

            builder.Register<PlayerPersistenceService>(Lifetime.Singleton)
                .As<IPlayerPersistence>();
            
            builder.Register<ChunkPersistenceService>(Lifetime.Singleton)
                .As<IChunkPersistence>();
            
            builder.Register<GamePersistenceCoordinator>(Lifetime.Singleton)
                .As<IGamePersistenceCoordinator>();

            builder.Register<DeltaApplicationStage>(Lifetime.Singleton)
                .As<IDeltaStage>();
        }
    }
}