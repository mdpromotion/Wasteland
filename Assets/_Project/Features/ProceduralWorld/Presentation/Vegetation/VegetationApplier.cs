using System.Collections.Generic;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Vegetation;
using _Project.Features.ProceduralWorld.Infrastructure.Vegetation;
using UnityEngine;

namespace _Project.Features.ProceduralWorld.Presentation.Vegetation
{
    public class VegetationApplier
    {
        private readonly VegetationSettingsProvider _provider;
        private readonly ChunkGrid _chunkGrid;

        private readonly Dictionary<VegetationSpeciesType, IReadOnlyList<GameObject>> _prefabCache;
        
        private const int DetailResolutionPerPatch = 128;
        private const int MaxDetailCoverage = 255;
        private const float CoverageContributionPerInstance = 48f;

        public VegetationApplier(VegetationSettingsProvider provider, ChunkGrid chunkGrid)
        {
            _provider = provider;
            _chunkGrid = chunkGrid;
            _prefabCache = new Dictionary<VegetationSpeciesType, IReadOnlyList<GameObject>>();
        }

        public void Apply(ChunkGenerationState state, Terrain terrain)
        {
            if (state.Vegetation == null)
                return;

            TerrainData terrainData = terrain.terrainData;

            terrainData.treeInstances = System.Array.Empty<TreeInstance>();
            TerrainCollider terrainCollider = EnsureTreeCollider(terrain);

            int resolution = (int)_chunkGrid.ChunkSizeX;
            float terrainHeight = terrainData.size.y;

            ApplyTrees(state, terrainData, resolution, terrainHeight);
            ApplyDetails(state, terrainData, resolution);

            RefreshTreeColliders(terrainCollider);
        }

        private void ApplyTrees(
            ChunkGenerationState state,
            TerrainData terrainData,
            int resolution,
            float terrainHeight)
        {
            List<TreePrototype> prototypes = new();
            Dictionary<VegetationSpeciesType, int[]> speciesPrototypeIndices = new();

            foreach (var layer in state.Vegetation.Layers)
            {
                if (layer == null || layer.Instances.Length == 0)
                    continue;

                if (_provider.GetRenderKind(layer.Species) != VegetationRenderKind.Tree)
                    continue;

                if (speciesPrototypeIndices.ContainsKey(layer.Species))
                    continue;

                IReadOnlyList<GameObject> prefabs = GetPrefabs(layer.Species);
                if (prefabs == null || prefabs.Count == 0)
                    continue;

                int[] indices = new int[prefabs.Count];

                for (int i = 0; i < prefabs.Count; i++)
                {
                    if (prefabs[i] == null)
                    {
                        indices[i] = -1;
                        continue;
                    }

                    indices[i] = prototypes.Count;
                    prototypes.Add(new TreePrototype { prefab = prefabs[i] });
                }

                speciesPrototypeIndices[layer.Species] = indices;
            }

            terrainData.treePrototypes = prototypes.ToArray();

            List<TreeInstance> treeInstances = new();

            foreach (var layer in state.Vegetation.Layers)
            {
                if (layer == null || layer.Instances.Length == 0)
                    continue;

                if (!speciesPrototypeIndices.TryGetValue(layer.Species, out int[] prototypeIndices))
                    continue;

                foreach (var instance in layer.Instances)
                {
                    int prototypeIndex = prototypeIndices[instance.Id % (uint)prototypeIndices.Length];

                    if (prototypeIndex < 0)
                        continue;

                    Vector3 normalizedPosition = new Vector3(
                        instance.Position.x / resolution,
                        instance.Position.y / terrainHeight,
                        instance.Position.z / resolution);

                    treeInstances.Add(new TreeInstance
                    {
                        position = normalizedPosition,
                        rotation = instance.Rotation,
                        prototypeIndex = prototypeIndex,
                        widthScale = instance.Scale,
                        heightScale = instance.Scale,
                        color = Color.white,
                        lightmapColor = Color.white,
                    });
                }
            }

            terrainData.SetTreeInstances(treeInstances.ToArray(), true);
        }

        private void ApplyDetails(
            ChunkGenerationState state,
            TerrainData terrainData,
            int resolution)
        {
            terrainData.SetDetailScatterMode(DetailScatterMode.CoverageMode);
            terrainData.SetDetailResolution(resolution, DetailResolutionPerPatch);

            int detailWidth = terrainData.detailWidth;
            int detailHeight = terrainData.detailHeight;

            List<DetailPrototype> detailPrototypes = new();
            List<(VegetationSpeciesType Species, int VariantIndex, int[,] Map)> pendingMaps = new();
            Dictionary<VegetationSpeciesType, int[]> speciesLayerIndices = new();

            bool hasAnyDetailLayer = false;

            foreach (var layer in state.Vegetation.Layers)
            {
                if (layer == null || layer.Instances.Length == 0) continue;
                if (_provider.GetRenderKind(layer.Species) != VegetationRenderKind.Detail) continue;
                if (speciesLayerIndices.ContainsKey(layer.Species)) continue;

                IReadOnlyList<GameObject> prefabs = GetPrefabs(layer.Species);
                if (prefabs == null || prefabs.Count == 0) continue;

                float minScale = float.MaxValue;
                float maxScale = float.MinValue;

                foreach (var instance in layer.Instances)
                {
                    if (instance.Scale < minScale) minScale = instance.Scale;
                    if (instance.Scale > maxScale) maxScale = instance.Scale;
                }

                if (minScale > maxScale)
                {
                    minScale = 0.8f;
                    maxScale = 1.2f;
                }
                else if (Mathf.Approximately(minScale, maxScale))
                {
                    minScale *= 0.8f;
                    maxScale *= 1.2f;
                }

                int[] layerIndices = new int[prefabs.Count];

                for (int i = 0; i < prefabs.Count; i++)
                {
                    if (!prefabs[i])
                    {
                        layerIndices[i] = -1;
                        continue;
                    }

                    layerIndices[i] = detailPrototypes.Count;

                    detailPrototypes.Add(new DetailPrototype
                    {
                        usePrototypeMesh = true,
                        prototype = prefabs[i],

                        renderMode = DetailRenderMode.VertexLit,
                        useInstancing = true,

                        healthyColor = Color.white,
                        dryColor = Color.white,

                        minWidth = minScale,
                        maxWidth = maxScale,
                        minHeight = minScale,
                        maxHeight = maxScale,

                        noiseSpread = 0.1f,
                        density = 1f,
                        alignToGround = 1f,
                        positionJitter = 0f,
                    });

                    pendingMaps.Add((layer.Species, i, new int[detailHeight, detailWidth]));
                    hasAnyDetailLayer = true;
                }

                speciesLayerIndices[layer.Species] = layerIndices;
            }

            if (!hasAnyDetailLayer)
            {
                terrainData.detailPrototypes = System.Array.Empty<DetailPrototype>();
                return;
            }

            terrainData.detailPrototypes = detailPrototypes.ToArray();

            var mapLookup = new Dictionary<(VegetationSpeciesType, int), int[,]>();
            foreach (var (species, variantIndex, map) in pendingMaps)
                mapLookup[(species, variantIndex)] = map;

            foreach (var layer in state.Vegetation.Layers)
            {
                if (layer == null || layer.Instances.Length == 0) continue;
                if (!speciesLayerIndices.TryGetValue(layer.Species, out int[] layerIndices)) continue;

                foreach (var instance in layer.Instances)
                {
                    int variantIndex = (int)(instance.Id % (uint)layerIndices.Length);
                    if (layerIndices[variantIndex] < 0) continue;

                    int[,] map = mapLookup[(layer.Species, variantIndex)];

                    float normalizedX = instance.Position.x / resolution;
                    float normalizedZ = instance.Position.z / resolution;

                    int cellX = math_clamp((int)(normalizedX * (detailWidth - 1)), 0, detailWidth - 1);
                    int cellZ = math_clamp((int)(normalizedZ * (detailHeight - 1)), 0, detailHeight - 1);
                    
                    int contribution = math_clamp(
                        Mathf.RoundToInt(instance.Scale * CoverageContributionPerInstance),
                        1,
                        MaxDetailCoverage);

                    map[cellZ, cellX] = math_clamp(map[cellZ, cellX] + contribution, 0, MaxDetailCoverage);
                }
            }
            
            foreach (var (species, variantIndex, map) in pendingMaps)
            {
                int layerIndex = speciesLayerIndices[species][variantIndex];
                if (layerIndex < 0) continue;

                terrainData.SetDetailLayer(0, 0, layerIndex, map);
            }
        }

        private static int math_clamp(int value, int min, int max) => value < min ? min : (value > max ? max : value);
        
        private static TerrainCollider EnsureTreeCollider(Terrain terrain)
        {
            TerrainCollider collider = terrain.GetComponent<TerrainCollider>();

            if (!collider)
                collider = terrain.gameObject.AddComponent<TerrainCollider>();

            collider.terrainData = terrain.terrainData;
            collider.enabled = true;

            return collider;
        }

        private static void RefreshTreeColliders(TerrainCollider collider)
        {
            if (!collider)
                return;

            collider.enabled = false;
            collider.enabled = true;
        }

        private IReadOnlyList<GameObject> GetPrefabs(VegetationSpeciesType species)
        {
            if (_prefabCache.TryGetValue(species, out var prefabs))
                return prefabs;

            prefabs = _provider.GetPrefabs(species);
            _prefabCache.Add(species, prefabs);
            return prefabs;
        }
    }
}