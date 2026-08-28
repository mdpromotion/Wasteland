using _Project.Features.Graphics.Domain;
using _Project.Features.ProceduralWorld.Application.Chunks;
using _Project.Features.ProceduralWorld.Application.Chunks.Generation;
using _Project.Features.ProceduralWorld.Application.World;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Hydrology;
using _Project.Features.ProceduralWorld.Domain.World;
using _Project.Features.ProceduralWorld.Infrastructure;
using _Project.Features.ProceduralWorld.Infrastructure.Hydrology;
using _Project.Features.ProceduralWorld.Infrastructure.Interfaces;
using _Project.Features.ProceduralWorld.Infrastructure.Landscape;
using _Project.Features.ProceduralWorld.Infrastructure.Vegetation;
using _Project.Features.ProceduralWorld.Infrastructure.Vegetation.Configs;
using System.Collections.Generic;
using _Project.Features.ProceduralWorld.Infrastructure.Chunks;
using _Project.Features.ProceduralWorld.Presentation.Hydrology;
using _Project.Features.ProceduralWorld.Presentation.Landscape;
using _Project.Features.ProceduralWorld.Presentation.Vegetation;
using _Project.Features.ProceduralWorld.Presentation.World;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Features.Core.Bootstrap.Game.Installers
{
    public static class ProceduralWorldInstaller
    {
        public static void Install(
            IContainerBuilder builder,
            Terrain chunkPrefab,
            MacroGridSettings macroGridSettings,
            RiverCarvingSettings riverCarvingSettings,
            VegetationCatalog vegetationCatalog,
            Material waterMaterial,
            Transform chunksParent,
            WorldRebaseSettings worldRebaseSettings)
        {
            builder.RegisterInstance(macroGridSettings);
            builder.RegisterInstance(riverCarvingSettings);
            builder.RegisterInstance(worldRebaseSettings);
            builder.RegisterInstance(vegetationCatalog);

            // Grid / caches

            builder.Register(
                    container => new ChunkGrid(
                        chunkPrefab.terrainData.size.x,
                        chunkPrefab.terrainData.size.z),
                    Lifetime.Singleton);

            builder.Register<MacroRegionCache>(Lifetime.Singleton);

            builder.Register<ChunkRepository>(Lifetime.Singleton)
                .As<IChunkLookup>()
                .AsSelf();

            // Appliers

            builder.Register(
                    container => new WaterSurfaceApplier(
                        container.Resolve<ChunkGrid>(),
                        chunkPrefab.terrainData.size.y,
                        waterMaterial),
                    Lifetime.Singleton)
                .As<IWaterSurfaceApplier>();

            builder.Register<VegetationApplier>(Lifetime.Singleton);

            builder.Register<TerrainNoiseSettingsProvider>(Lifetime.Singleton);

            builder.Register<VegetationSettingsProvider>(Lifetime.Singleton);

            builder.Register<UnityTerrainWriter>(Lifetime.Singleton)
                .As<ITerrainWriter>();

            builder.Register<ChunkNeighborConnector>(Lifetime.Singleton);

            builder.Register(
                    container => new LandscapeChunkFactory(
                        chunkPrefab,
                        container.Resolve<ChunkGrid>(),
                        container.Resolve<GraphicsState>()),
                    Lifetime.Singleton)
                .As<ILandscapeFactory>();

            builder.Register(
                    container => new LandscapeApplier(
                        container.Resolve<ILandscapeFactory>(),
                        container.Resolve<ITerrainWriter>(),
                        container.Resolve<ChunkNeighborConnector>(),
                        container.Resolve<ChunkRepository>(),
                        container.Resolve<IWaterSurfaceApplier>(),
                        container.Resolve<VegetationApplier>(),
                        chunksParent),
                    Lifetime.Singleton)
                .As<ILandscapeApplier>();

            builder.Register<WorldRebaseApplier>(Lifetime.Singleton)
                .As<IWorldRebaseApplier>();

            // Generation pipeline

            builder.Register<LandscapeGenerator>(Lifetime.Singleton)
                .As<IGenerationStage>();
            
            builder.Register<HydrologyGenerator>(Lifetime.Singleton)
                .As<IGenerationStage>();

            builder.Register<WaterSurfaceStage>(Lifetime.Singleton)
                .As<IGenerationStage>();

            builder.Register<VegetationGenerator>(Lifetime.Singleton)
                .As<IGenerationStage>();

            builder.Register<ChunkGenerationPipeline>(Lifetime.Singleton)
                .AsSelf()
                .As<IChunkGenerator>();
            
            builder.Register<WorldSpaceConverter>(Lifetime.Singleton)
                .As<IWorldSpaceConverter>();
            
            builder.Register<BreakableQueryService>(Lifetime.Singleton)
                .As<IBreakableQuery>();

            builder.RegisterBuildCallback(container =>
            {
                ChunkGenerationPipeline pipeline =
                    container.Resolve<ChunkGenerationPipeline>();

                foreach (IGenerationStage stage
                         in container.Resolve<IEnumerable<IGenerationStage>>())
                {
                    pipeline.Add(stage);
                }
            });

            // Streaming / management

            builder.Register<ChunkGenerationScheduler>(Lifetime.Singleton);

            builder.Register<ChunkManager>(Lifetime.Singleton)
                .As<IChunkManager>()
                .As<ITickable>()
                .AsSelf();

            builder.Register<WorldRebaseService>(Lifetime.Singleton);

            builder.Register<WorldStreamer>(Lifetime.Singleton)
                .As<IInitializable>()
                .As<ITickable>();
        }
    }
}