using System;

namespace _Project.Features.ProceduralWorld.Domain.Hydrology
{
    /// <summary>
    /// Identifies a region in the macro-scale hydrology grid.
    /// </summary>
    /// <remarks>
    /// Macro-region coordinates are separate from chunk coordinates and identify
    /// the larger regions used to maintain hydrology continuity across chunks.
    /// </remarks>
    public readonly struct MacroRegionCoordinate : IEquatable<MacroRegionCoordinate>
    {
        public readonly int X;
        public readonly int Y;

        public MacroRegionCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(MacroRegionCoordinate other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is MacroRegionCoordinate other && Equals(other);
        public override int GetHashCode() => (X * 397) ^ Y;
        
        /// <summary>
        /// Returns the coordinate in <c>MacroRegion(X,Y)</c> format.
        /// </summary>
        public override string ToString() => $"MacroRegion({X},{Y})";
    }
}