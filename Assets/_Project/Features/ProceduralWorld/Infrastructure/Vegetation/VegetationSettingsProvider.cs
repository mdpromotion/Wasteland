using System.Collections.Generic;
using System.Linq;
using _Project.Features.ProceduralWorld.Domain.Vegetation;
using _Project.Features.ProceduralWorld.Infrastructure.Vegetation.Configs;
using UnityEngine;

namespace _Project.Features.ProceduralWorld.Infrastructure.Vegetation
{
    public enum VegetationRenderKind
    {
        None,
        Tree,
        Detail
    }
    
    public sealed class VegetationSettingsProvider
    {
        private readonly VegetationCatalog _catalog;

        public VegetationSettingsProvider(VegetationCatalog catalog)
        {
            _catalog = catalog;
        }
        
        public IEnumerable<(VegetationSpeciesType Species, VegetationGenerationParams Params)> Create()
        {
            var species = new List<(VegetationSpeciesType Species, VegetationGenerationParams Params)>();
            
            foreach (var config in _catalog.Species)
            {
                var generationParams = new VegetationGenerationParams
                {
                    Coverage = config.Coverage,
                    Density = config.Density,
                    EdgeSmoothing = config.EdgeSmooting,
                    MinScale =  config.MinScale,
                    MaxScale = config.MaxScale,
                    MinSlopeAngle =  config.MinSlopeAngle,
                    MaxSlopeAngle = config.MaxSlopeAngle,
                    PatchNoiseFrequency = config.PatchNoiseFrequency,
                    PatchNoiseOctaves = config.PatchNoiseOctaves,
                    Priority = config.Priority,
                    IsBreakable = config.IsBreakable,
                    OccupancyRadius = config.OccupancyRadius
                };
                
                species.Add((config.SpeciesType, generationParams));
            }
            
            return species;
        }
        
        public IReadOnlyList<GameObject> GetPrefabs(VegetationSpeciesType speciesType)
        {
            var config = _catalog.Species
                .FirstOrDefault(x => x.SpeciesType == speciesType);

            return config != null ? config.Prefabs : System.Array.Empty<GameObject>();
        }

        public VegetationRenderKind GetRenderKind(VegetationSpeciesType speciesType)
        {
            var config = _catalog.Species
                .FirstOrDefault(x => x.SpeciesType == speciesType);

            if (config == null) return VegetationRenderKind.None;
            
            return config.IsDetail ? VegetationRenderKind.Detail : VegetationRenderKind.Tree;
        }
    }
}