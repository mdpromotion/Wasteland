using _Project.Features.Player.Application;
using _Project.Features.Player.Application.UseCases;
using _Project.Features.Player.Domain;
using _Project.Features.Player.Infrastructure;
using _Project.Features.Player.Presentation;
using _Project.Features.ProceduralWorld.Application.Chunks;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Vegetation;
using _Project.Features.ProceduralWorld.Infrastructure.Hydrology;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Features.Core.Bootstrap.Game.Installers
{
    public static class PlayerInstaller
    {
        public static void Install(
            IContainerBuilder builder,
            PlayerMovementConfig playerMovementConfig,
            PlayerSoundSet playerSoundSet,
            Terrain chunkPrefab)
        {
            builder.RegisterInstance(playerMovementConfig)
                .AsSelf();

            builder.Register<GroundMovementUseCase>(Lifetime.Singleton);

            builder.Register<SwimmingMovementUseCase>(Lifetime.Singleton);

            builder.RegisterComponentInHierarchy<PlayerStanceController>()
                .As<IPlayerStanceState>();

            builder.RegisterComponentInHierarchy<FpsPlayerMotor>()
                .As<IFpsPlayerMotor>();

            builder.Register<PlayerEnvironmentState>(Lifetime.Singleton)
                .As<IPlayerEnvironmentState>()
                .AsSelf();

            builder.Register<PlayerController>(Lifetime.Singleton)
                .As<IFixedTickable>()
                .As<IPlayerController>();

            builder.Register<PlayerWorldRebaseSync>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterComponentInHierarchy<RigidbodyPlayerState>()
                .As<IPlayerReadOnly>();

            builder.RegisterComponentInHierarchy<WaterVolumeTracker>()
                .As<IWaterState>();

            builder.RegisterComponentInHierarchy<PlayerWaterSoundController>();

            builder.Register(
                container => new WaterQueryService(
                    container.Resolve<ChunkGrid>(),
                    container.Resolve<IChunkLookup>(),
                    chunkPrefab.terrainData.size.y),
                Lifetime.Singleton)
                .As<IWaterQuery>();

            builder.RegisterInstance(playerSoundSet);

            builder.RegisterComponentInHierarchy<PlayerBreakableInteractor>();
            builder.RegisterComponentInHierarchy<FootstepController>();
        }
    }
}