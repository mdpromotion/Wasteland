using Unity.Mathematics;
using _Project.Features.ProceduralWorld.Domain.Chunks;

namespace _Project.Features.ProceduralWorld.Domain.Vegetation
{
    public readonly struct BreakableHit
    {
        public readonly ChunkCoordinate Coordinate;
        public readonly VegetationSpeciesType Species;
        public readonly int LayerIndex;
        public readonly int InstanceIndex;
        public readonly float3 WorldPosition;
        public readonly ulong Id;

        public BreakableHit(
            ChunkCoordinate coordinate,
            VegetationSpeciesType species,
            int layerIndex,
            int instanceIndex,
            float3 worldPosition,
            ulong id)
        {
            Coordinate = coordinate;
            Species = species;
            LayerIndex = layerIndex;
            InstanceIndex = instanceIndex;
            WorldPosition = worldPosition;
            Id = id;
        }
    }
}