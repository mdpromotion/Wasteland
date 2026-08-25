using UnityEngine;

namespace _Project.Features.ProceduralWorld.Domain.Chunks
{
    /// <summary>
    /// Converts between logical chunk coordinates and world-space positions relative
    /// to the current chunk-grid origin.
    /// </summary>
    /// <remarks>
    /// The grid uses X/Z for world-space coordinates while <see cref="ChunkCoordinate"/>
    /// stores the two-dimensional grid position as X/Y. Changing the origin allows the
    /// world to be rebased without changing the logical coordinates of existing chunks.
    /// </remarks>
    public class ChunkGrid
    {  
        /// <summary>
        /// Width of a chunk along the world X axis.
        /// </summary>
        public float ChunkSizeX { get; }

        /// <summary>
        /// Depth of a chunk along the world Z axis.
        /// </summary>
        public float ChunkSizeZ { get; }

        /// <summary>
        /// Logical chunk coordinate currently used as the grid origin.
        /// </summary>
        public ChunkCoordinate OriginCoordinate { get; private set; }

        public ChunkGrid(float chunkSizeX, float chunkSizeZ)
        {
            ChunkSizeX = chunkSizeX;
            ChunkSizeZ = chunkSizeZ;
            OriginCoordinate = new ChunkCoordinate(0, 0);
        }

        /// <summary>
        /// Converts a logical chunk coordinate into a world-space X/Z offset
        /// relative to <see cref="OriginCoordinate"/>.
        /// </summary>
        public Vector2 ToWorldOffset(ChunkCoordinate coordinate)
        {
            return new Vector2(
                (coordinate.X - OriginCoordinate.X) * ChunkSizeX,
                (coordinate.Y - OriginCoordinate.Y) * ChunkSizeZ);
        }
        
        /// <summary>
        /// Converts a world-space position into the logical chunk containing that position.
        /// </summary>
        /// <remarks>
        /// The world Y coordinate is ignored. Negative positions are mapped using floor
        /// semantics so that each world position belongs to exactly one chunk.
        /// </remarks>
        public ChunkCoordinate ToChunkCoordinate(Vector3 worldPosition)
        {
            int relativeX = Mathf.FloorToInt(worldPosition.x / ChunkSizeX);
            int relativeY = Mathf.FloorToInt(worldPosition.z / ChunkSizeZ);

            return new ChunkCoordinate(
                relativeX + OriginCoordinate.X,
                relativeY + OriginCoordinate.Y);
        }

        public void SetOriginCoordinate(ChunkCoordinate origin)
        {
            OriginCoordinate = origin;
        }
    }
}