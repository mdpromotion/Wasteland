using System.Collections.Generic;

namespace _Project.Features.ProceduralWorld.Domain.Chunks
{
    /// <summary>
    /// Compares chunk coordinates by their distance from a configurable reference coordinate.
    /// </summary>
    /// <remarks>
    /// Used by the generation scheduler to prioritize chunks closer to the current
    /// streaming center.
    /// </remarks>
    public sealed class ChunkCoordinateDistanceComparer : IComparer<ChunkCoordinate>
    {
        public ChunkCoordinate Center;

        public int Compare(ChunkCoordinate a, ChunkCoordinate b)
        {
            int da = (a.X - Center.X) * (a.X - Center.X) +
                     (a.Y - Center.Y) * (a.Y - Center.Y);

            int db = (b.X - Center.X) * (b.X - Center.X) +
                     (b.Y - Center.Y) * (b.Y - Center.Y);

            return da.CompareTo(db);
        }
    }
}