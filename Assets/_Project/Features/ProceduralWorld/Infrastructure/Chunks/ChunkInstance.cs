using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Hydrology;
using _Project.Features.ProceduralWorld.Domain.Landscape;
using _Project.Features.ProceduralWorld.Domain.Vegetation;
using UnityEngine;
 
namespace _Project.Features.ProceduralWorld.Infrastructure.Chunks
{
    public sealed class ChunkInstance
    {
        public ChunkCoordinate Coordinate { get; }
        public LandscapeData Landscape { get; }
        public HydrologyData Hydrology { get; } 
        public VegetationData Vegetation { get; }
        public Terrain Terrain { get; }
 
        public ChunkInstance(
            ChunkCoordinate coordinate,
            LandscapeData landscape,
            HydrologyData hydrology,
            VegetationData vegetation,
            Terrain terrain)
        {
            Coordinate = coordinate;
            Landscape = landscape;
            Hydrology = hydrology;
            Vegetation = vegetation;
            Terrain = terrain;
        }
    }
}