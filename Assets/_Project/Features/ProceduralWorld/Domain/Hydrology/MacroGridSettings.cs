using UnityEngine;

namespace _Project.Features.ProceduralWorld.Domain.Hydrology
{
    /// <summary>
    /// Configuration for the macro-scale hydrology grid used to generate continuous
    /// water and river features across chunk boundaries.
    /// </summary>
    /// <remarks>
    /// The grid uses padded regions so that hydrology calculations near a region edge
    /// have access to neighboring terrain data before the core region is extracted.
    /// </remarks>
    [CreateAssetMenu(menuName = "Procedural World/Macro Grid Settings")]
    public sealed class MacroGridSettings : ScriptableObject
    {
        /// <summary>
        /// Number of world-space units represented by one macro-grid cell.
        /// </summary>
        public float CellSize;

        /// <summary>
        /// Number of cells in the unpadded macro region.
        /// </summary>
        public int TileCells;

        /// <summary>
        /// Number of padding cells added around each macro region.
        /// </summary>
        public int PaddingCells;

        public int RiverZoneMargin = 1;

        public float EdgeBiasStrength = 2f;

        public int PaddedSize => TileCells + 2 * PaddingCells;

        public float TileWorldSize => TileCells * CellSize;

        [Header("River Strength")]
        public float LocalAccumulationNormalizationRange = 10f;

        public int EmbankmentSmoothingRadius = 3;

        public int CoreSize => PaddedSize - 2 * RiverZoneMargin;
    }
}