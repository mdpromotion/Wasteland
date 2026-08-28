using System;
using _Project.Features.ProceduralWorld.Domain.World;

namespace _Project.Features.ProceduralWorld.Domain.Chunks
{
    public static class ChunkGridExtensions
    {
        /// <summary>
        /// Converts an absolute world position into the logical chunk containing it.
        /// </summary>
        public static ChunkCoordinate ToChunkCoordinate(
            this ChunkGrid grid,
            WorldPosition worldPosition)
        {
            int cx = (int)Math.Floor(worldPosition.X / grid.ChunkSizeX);
            int cz = (int)Math.Floor(worldPosition.Z / grid.ChunkSizeZ);

            return new ChunkCoordinate(cx, cz);
        }
    }
}