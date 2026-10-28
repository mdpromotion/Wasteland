using _Project.Features.ProceduralWorld.Application.Chunks.Generation;
using _Project.Features.ProceduralWorld.Domain;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Hydrology;
using _Project.Features.ProceduralWorld.Infrastructure.Jobs.Hydrology;
using Unity.Jobs;
using Unity.Mathematics;

namespace _Project.Features.ProceduralWorld.Infrastructure.Hydrology
{
    public sealed class HydrologyGenerator : IGenerationStage
    {
        private readonly ChunkGrid _chunkGrid;
        private readonly MacroRegionCache _macroRegionCache;
        private readonly MacroGridSettings _macroGridSettings;

        public HydrologyGenerator(
            ChunkGrid chunkGrid,
            MacroRegionCache macroRegionCache,
            MacroGridSettings macroGridSettings)
        {
            _chunkGrid = chunkGrid;
            _macroRegionCache = macroRegionCache;
            _macroGridSettings = macroGridSettings;
        }

        public JobHandle Schedule(ChunkGenerationState state, JobHandle dependency)
        {
            int resolution = state.Context.Resolution;
            ChunkCoordinate coordinate = state.Context.Coordinate;

            double2 absoluteChunkOrigin = GenerationSpace.AbsoluteChunkOrigin(
                coordinate, _chunkGrid.ChunkSizeX, _chunkGrid.ChunkSizeZ);

            MacroRegionCoordinate regionCoordinate = _macroRegionCache.ToRegionCoordinate(coordinate);
            MacroRegionData region = _macroRegionCache.GetOrBuild(regionCoordinate);

            if (!region.IsCreated)
            {
                throw new System.InvalidOperationException(
                    $"Macro-region {regionCoordinate.X}, {regionCoordinate.Y} returned disposed native data " +
                    $"for chunk {coordinate.X}, {coordinate.Y}.");
            }

            state.Hydrology = new HydrologyData(coordinate, resolution, onDispose: null);

            float2 chunkOrigin = GenerationSpace.LocalOffset(absoluteChunkOrigin, region.WorldOrigin);
            float2 chunkSize = new float2(_chunkGrid.ChunkSizeX, _chunkGrid.ChunkSizeZ);

            var job = new ComputeRiverStrengthJob
            {
                Resolution = resolution,
                ChunkWorldOrigin = chunkOrigin,
                ChunkWorldSize = chunkSize,

                MacroPaddedSize = region.PaddedSize,
                MacroPaddingCells = _macroGridSettings.PaddingCells,
                MacroTileCells = _macroGridSettings.TileCells,
                MacroRiverZoneMargin = _macroGridSettings.RiverZoneMargin,
                MacroCellSize = region.CellSize,
                MacroWorldOrigin = float2.zero,

                MacroRiverStrengthTight = region.RiverStrengthTight,
                MacroRiverStrengthSmoothed = region.RiverStrengthSmoothed,
                MacroHeights = region.Heights,
                MacroWaterLevels = region.WaterLevels,

                RiverStrength = state.Hydrology.Accumulation,
                EmbankmentStrength = state.Hydrology.EmbankmentStrength,
                MacroHeightSample = state.Hydrology.MacroHeightSample,
                WaterSurfaceHeight = state.Hydrology.WaterSurfaceHeight
            };

            return job.Schedule(resolution * resolution, 64, dependency);
        }
    }
}
