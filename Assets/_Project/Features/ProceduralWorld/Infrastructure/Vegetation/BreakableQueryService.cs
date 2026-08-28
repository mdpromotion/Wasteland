using System.Collections.Generic;
using _Project.Features.ProceduralWorld.Application.Chunks;
using Unity.Collections;
using Unity.Mathematics;
using _Project.Features.ProceduralWorld.Domain;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Vegetation;
using _Project.Features.ProceduralWorld.Domain.World;
using _Project.Features.ProceduralWorld.Infrastructure.Chunks;
using UnityEngine;

namespace _Project.Features.ProceduralWorld.Infrastructure.Vegetation
{
    public interface IBreakableQuery
    {
        bool TryFindBreakable(WorldPosition absolutePosition, float radius, out BreakableHit hit);
    }

    public sealed class BreakableQueryService : IBreakableQuery
    {
        private readonly ChunkGrid _grid;
        private readonly IChunkLookup _chunkLookup;

        public BreakableQueryService(ChunkGrid grid, IChunkLookup chunkLookup)
        {
            _grid = grid;
            _chunkLookup = chunkLookup;
        }

        public bool TryFindBreakable(WorldPosition absolutePosition, float radius, out BreakableHit hit)
        {
            hit = default;

            double absoluteX = absolutePosition.X;
            double absoluteZ = absolutePosition.Z;

            ChunkCoordinate currentRebaseOrigin = _grid.OriginCoordinate;
            double2 rebaseOriginWorldPos = GenerationSpace.AbsoluteChunkOrigin(
                currentRebaseOrigin, _grid.ChunkSizeX, _grid.ChunkSizeZ);

            double localX = absoluteX - rebaseOriginWorldPos.x;
            double localZ = absoluteZ - rebaseOriginWorldPos.y;

            ChunkCoordinate coordinate = _grid.ToChunkCoordinate(new Vector3((float)localX, 0f, (float)localZ));

            if (!_chunkLookup.TryGet(coordinate, out ChunkInstance chunk))
                return false;

            VegetationData vegetation = chunk.Vegetation;
            if (vegetation == null)
                return false;

            double2 chunkOriginAbs = GenerationSpace.AbsoluteChunkOrigin(
                coordinate, _grid.ChunkSizeX, _grid.ChunkSizeZ);
            
            float2 localTargetInChunk = GenerationSpace.LocalOffset(
                new double2(absoluteX, absoluteZ), chunkOriginAbs);

            float bestSqrDistance = radius * radius;
            bool found = false;
            BreakableHit bestHit = default;

            IReadOnlyList<VegetationLayerData> layers = vegetation.Layers;
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                VegetationLayerData layer = layers[layerIndex];
                NativeList<VegetationInstanceData> instances = layer.Instances;

                if (!instances.IsCreated)
                    continue;

                for (int i = 0; i < instances.Length; i++)
                {
                    VegetationInstanceData instance = instances[i];
                    if (!instance.IsBreakable)
                        continue;
                    
                    float2 instanceXZ = new float2(instance.Position.x, instance.Position.z);
                    float sqrDistance = math.distancesq(instanceXZ, localTargetInChunk);

                    if (sqrDistance >= bestSqrDistance)
                        continue;

                    bestSqrDistance = sqrDistance;
                    found = true;
                    bestHit = new BreakableHit(coordinate, layer.Species, layerIndex, i, instance.Position, instance.Id);
                }
            }

            hit = bestHit;

            return found;
        }
    }
}