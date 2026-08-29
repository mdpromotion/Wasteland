using UnityEngine;
using Unity.Mathematics;
using _Project.Features.ProceduralWorld.Domain;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.World;

namespace _Project.Features.ProceduralWorld.Infrastructure.Chunks
{
    public interface IWorldSpaceConverter
    {
        WorldPosition ToWorldPosition(Vector3 rebasedPosition);
    }

    public sealed class WorldSpaceConverter : IWorldSpaceConverter
    {
        private readonly ChunkGrid _chunkGrid;

        public WorldSpaceConverter(ChunkGrid chunkGrid)
        {
            _chunkGrid = chunkGrid;
        }

        public WorldPosition ToWorldPosition(Vector3 rebasedPosition)
        {
            double2 originAbs = GenerationSpace.AbsoluteChunkOrigin(
                _chunkGrid.OriginCoordinate,
                _chunkGrid.ChunkSizeX,
                _chunkGrid.ChunkSizeZ);

            return new WorldPosition(
                originAbs.x + rebasedPosition.x,
                rebasedPosition.y,
                originAbs.y + rebasedPosition.z);
        }
    }
}