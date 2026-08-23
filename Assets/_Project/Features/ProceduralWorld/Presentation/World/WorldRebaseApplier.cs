using _Project.Features.ProceduralWorld.Infrastructure.Chunks;
using UnityEngine;

namespace _Project.Features.ProceduralWorld.Presentation.World
{
    public interface IWorldRebaseApplier
    {
        void MoveChunkTo(ChunkInstance chunk, Vector3 delta);
        void SyncTransforms();
    }
    
    public sealed class WorldRebaseApplier : IWorldRebaseApplier
    {
        public void MoveChunkTo(ChunkInstance chunk, Vector3 delta)
        {
            if (chunk.Terrain)
                chunk.Terrain.transform.position += delta;
        }

        public void SyncTransforms()
        {
            Physics.SyncTransforms();
        }
    }
}