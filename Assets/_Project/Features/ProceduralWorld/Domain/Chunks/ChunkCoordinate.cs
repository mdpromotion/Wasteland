using System;

namespace _Project.Features.ProceduralWorld.Domain.Chunks
{
    /// <summary>
    /// Identifies a chunk in the logical procedural-world grid.
    /// </summary>
    /// <remarks>
    /// The coordinate represents chunk indices rather than Unity world-space units.
    /// When converted to world space, <csharp>X</csharp> maps to the world X axis and
    /// <csharp>Y</csharp> maps to the world Z axis.
    /// </remarks>
    public readonly struct ChunkCoordinate : IEquatable<ChunkCoordinate>
    {
        public readonly int X;
        public readonly int Y;
        
        
        /// <summary>
        /// Creates a logical chunk coordinate.
        /// </summary>
        /// <param name="x">Chunk index along the world X axis.</param>
        /// <param name="y">Chunk index along the world Z axis.</param>
        public ChunkCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }


        public bool Equals(ChunkCoordinate other)
        {
            return X == other.X &&
                   Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkCoordinate other &&
                   Equals(other);
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        /// <summary>
        /// Adds two chunk coordinates component-wise.
        /// </summary>
        /// <remarks>
        /// This is useful for expressing relative chunk positions such as neighbors
        /// and offsets within the streaming grid.
        /// </remarks>
        public static ChunkCoordinate operator +
        (
            ChunkCoordinate a,
            ChunkCoordinate b
        )
        {
            return new ChunkCoordinate(
                a.X + b.X,
                a.Y + b.Y);
        }
    }
}