using System.Collections.Generic;
using System.Linq;
using _Project.Features.ProceduralWorld.Application.Chunks.Generation;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Vegetation;
using _Project.Features.ProceduralWorld.Domain.World;
using _Project.Features.ProceduralWorld.Infrastructure.Jobs.Vegetation;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace _Project.Features.ProceduralWorld.Infrastructure.Vegetation
{
    public class VegetationGenerator : IGenerationStage
    {
        private readonly VegetationSettingsProvider _settingsProvider;
        private readonly IWorldSettings _worldSettings;

        private List<(VegetationSpeciesType Species, VegetationGenerationParams Params)> _speciesInPriorityOrder;
        private bool _isPrioritized = false;

        public VegetationGenerator(VegetationSettingsProvider settingsProvider, IWorldSettings worldSettings)
        {
            _worldSettings = worldSettings;
            _settingsProvider = settingsProvider;
            _speciesInPriorityOrder = new List<(VegetationSpeciesType, VegetationGenerationParams)>();
        }

        public JobHandle Schedule(ChunkGenerationState state, JobHandle dependency)
        {
            ChunkGenerationContext context = state.Context;
            int2 chunkCoordinate = new int2(context.Coordinate.X, context.Coordinate.Y);

            int resolution = state.Landscape.Resolution;
            int cellCount = resolution * resolution;

            NativeArray<byte> occupancy = new NativeArray<byte>(
                cellCount,
                Allocator.Persistent);

            var speciesOrder = GetOrPrioritizeOrder();

            var layers = new List<VegetationLayerData>(speciesOrder.Count);
            JobHandle chain = dependency;

            foreach (var (species, generationParams) in speciesOrder)
            {
                NativeArray<VegetationInstanceData> candidates = new NativeArray<VegetationInstanceData>(
                    cellCount,
                    Allocator.Persistent);

                NativeArray<byte> candidateMask = new NativeArray<byte>(
                    cellCount,
                    Allocator.Persistent);

                NativeList<VegetationInstanceData> accepted = new NativeList<VegetationInstanceData>(
                    cellCount / 4,
                    Allocator.Persistent);

                VegetationCandidateJob candidateJob = new VegetationCandidateJob(
                    resolution,
                    chunkCoordinate,
                    generationParams,
                    state.Landscape.Heights,
                    state.Landscape.Resolution,
                    state.Hydrology.WaterSurfaceHeight,
                    state.Hydrology.RiverMask,
                    _worldSettings.Seed,
                    candidates,
                    candidateMask);

                JobHandle stageA = candidateJob.Schedule(cellCount, 64, chain);

                VegetationCommitJob commitJob = new VegetationCommitJob(
                    resolution,
                    generationParams.OccupancyRadius,
                    occupancy,
                    candidates,
                    candidateMask,
                    accepted);

                JobHandle stageB = commitJob.Schedule(stageA);

                candidates.Dispose(stageB);
                candidateMask.Dispose(stageB);

                layers.Add(new VegetationLayerData(species, accepted));

                chain = stageB;
            }

            state.Vegetation = new VegetationData(context.Coordinate, layers);

            occupancy.Dispose(chain);

            return chain;
        }

        private List<(VegetationSpeciesType Species, VegetationGenerationParams Params)> GetOrPrioritizeOrder()
        {
            if (_isPrioritized)
                return _speciesInPriorityOrder;

            var species = _settingsProvider.Create();

            _speciesInPriorityOrder = species.OrderBy(s => s.Params.Priority).ToList();
            _isPrioritized = true;

            return _speciesInPriorityOrder;
        }
    }
}