using _Project.Features.Core.Bootstrap.Game.Installers;
using _Project.Features.Player.Infrastructure;
using _Project.Features.ProceduralWorld.Infrastructure.Vegetation.Configs;
using _Project.Features.ProceduralWorld.Domain.Hydrology;
using _Project.Features.ProceduralWorld.Domain.World;
using _Project.Features.Sound.Infrastructure;
using _Project.Features.Camera.Infrastructure;
using _Project.Features.Core.Infrastructure;
using _Project.Features.ProceduralWorld.Infrastructure.Hydrology;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Features.Core.Bootstrap.Game
{
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("Procedural World")]
        [SerializeField] private Terrain chunkPrefab;
        [SerializeField] private MacroGridSettings macroGridSettings;
        [SerializeField] private RiverCarvingSettings riverCarvingSettings;
        [SerializeField] private VegetationCatalog vegetationCatalog;
        [SerializeField] private Material waterMaterial;
        [SerializeField] private Transform chunksParent;
        [SerializeField] private WorldRebaseSettings worldRebaseSettings;

        [Header("Sound")]
        [SerializeField] private SoundDatabase database;
        [SerializeField] private int globalMaxVoices = 32;
        [SerializeField] private PlayerSoundSet playerSoundSet;

        [Header("Player")]
        [SerializeField] private PlayerMovementConfig playerMovementConfig;
        [SerializeField] private PlayerCameraConfig playerCameraConfig;

        [Header("Performance")]
        [SerializeField] private FrameBudgetConfig frameBudgetConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            SoundInstaller.Install(
                builder,
                database,
                globalMaxVoices);

            PlayerInstaller.Install(
                builder,
                playerMovementConfig,
                playerSoundSet,
                chunkPrefab);

            CameraInstaller.Install(
                builder,
                playerCameraConfig);

            TickInstaller.Install(builder);

            GameTimeInstaller.Install(builder);

            PersistenceInstaller.Install(builder);

            ProceduralWorldInstaller.Install(
                builder,
                chunkPrefab,
                macroGridSettings,
                riverCarvingSettings,
                vegetationCatalog,
                waterMaterial,
                chunksParent,
                worldRebaseSettings);

            CoreInstaller.Install(
                builder,
                frameBudgetConfig);

            UIInstaller.Install(builder);
        }
    }
}