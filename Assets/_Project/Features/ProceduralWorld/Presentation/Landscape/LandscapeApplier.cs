using _Project.Features.ProceduralWorld.Application.Chunks;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Landscape;
using _Project.Features.ProceduralWorld.Infrastructure;
using _Project.Features.ProceduralWorld.Infrastructure.Chunks;
using _Project.Features.ProceduralWorld.Infrastructure.Hydrology;
using _Project.Features.ProceduralWorld.Infrastructure.Interfaces;
using _Project.Features.ProceduralWorld.Presentation.Hydrology;
using _Project.Features.ProceduralWorld.Presentation.Vegetation;
using UnityEngine;

namespace _Project.Features.ProceduralWorld.Presentation.Landscape
{
    public interface ILandscapeApplier
    {
        void Apply(ChunkGenerationResult result);
    }
    
    public class LandscapeApplier : ILandscapeApplier
    {
        private readonly ILandscapeFactory _factory;
        private readonly ITerrainWriter _writer;
        private readonly ChunkNeighborConnector _neighborConnector;
        private readonly ChunkRepository _repository;
        private readonly IWaterSurfaceApplier _waterSurfaceApplier;
        private readonly VegetationApplier _vegetationApplier;
        private readonly Transform _parent;

        public LandscapeApplier(
            ILandscapeFactory factory,
            ITerrainWriter writer,
            ChunkNeighborConnector neighborConnector,
            ChunkRepository repository,
            IWaterSurfaceApplier waterSurfaceApplier,
            VegetationApplier vegetationApplier,
            Transform parent)
        {
            _factory = factory;
            _writer = writer;
            _neighborConnector = neighborConnector;
            _repository = repository;
            _waterSurfaceApplier = waterSurfaceApplier;
            _vegetationApplier = vegetationApplier;
            _parent = parent;
        }

        public void Apply(ChunkGenerationResult result)
        {
            ChunkGenerationState state = result.State;
            LandscapeData data = state.Landscape;

            Terrain terrain = _factory.Create(data.Coordinate, _parent);

            _writer.Write(terrain, data);
            terrain.terrainData.SyncHeightmap();

            _waterSurfaceApplier.Apply(state, terrain);
            _vegetationApplier.Apply(state, terrain);

            ChunkInstance chunk = new ChunkInstance(data.Coordinate, data, state.Hydrology, state.Vegetation, terrain);

            _repository.Add(chunk);
            _neighborConnector.Connect(_repository, data.Coordinate);
        }
    }
}