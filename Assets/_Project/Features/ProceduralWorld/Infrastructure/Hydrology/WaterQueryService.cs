using UnityEngine;
using _Project.Features.ProceduralWorld.Application.Chunks;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Hydrology;
using _Project.Features.ProceduralWorld.Infrastructure.Chunks;

namespace _Project.Features.ProceduralWorld.Infrastructure.Hydrology
{
    public interface IWaterQuery
    {
        bool TryGetWaterState(Vector3 worldPosition, out WaterSample sample);
    }
    
    public sealed class WaterQueryService : IWaterQuery
    {
        private readonly ChunkGrid _grid;
        private readonly IChunkLookup _chunkLookup;
        private readonly float _heightScale;

        public WaterQueryService(
            ChunkGrid grid,
            IChunkLookup chunkLookup,
            float heightScale)
        {
            _grid = grid;
            _chunkLookup = chunkLookup;
            _heightScale = heightScale;
        }

        public bool TryGetWaterState(
            Vector3 worldPosition,
            out WaterSample sample)
        {
            ChunkCoordinate coordinate = _grid.ToChunkCoordinate(worldPosition);
            if (!_chunkLookup.TryGet(coordinate, out ChunkInstance chunk))
            {
                sample = default;
                return false;
            }

            HydrologyData hydrology = chunk.Hydrology;
            if (!hydrology.RiverMask.IsCreated ||
                !hydrology.WaterSurfaceHeight.IsCreated)
            {
                sample = default;
                return false;
            }

            Vector2 chunkOrigin = _grid.ToWorldOffset(coordinate);
            float localX = worldPosition.x - chunkOrigin.x;
            float localZ = worldPosition.z - chunkOrigin.y;

            float u = Mathf.Clamp01(localX / _grid.ChunkSizeX);
            float v = Mathf.Clamp01(localZ / _grid.ChunkSizeZ);

            int resolution = hydrology.Resolution;
            float gx = u * (resolution - 1);
            float gz = v * (resolution - 1);

            float mask = HydrologySampler.SampleBilinear(
                hydrology.RiverMask,
                resolution,
                gx,
                gz);

            float normalizedSurfaceHeight = HydrologySampler.SampleBilinear(
                hydrology.WaterSurfaceHeight,
                resolution,
                gx,
                gz);

            float worldSurfaceHeight = normalizedSurfaceHeight * _heightScale;
            sample = new WaterSample(mask, worldSurfaceHeight);
            return true;
        }
    }
}