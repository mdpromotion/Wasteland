using Unity.Mathematics;

namespace _Project.Features.ProceduralWorld.Domain.Vegetation
{
    /// <summary>
    /// Creates collision-free deterministic identifiers for vegetation instances.
    ///
    /// The global cell coordinates are encoded reversibly into a single ulong.
    /// This is not a hash, so different int2 coordinates cannot produce the same Id.
    /// </summary>
    public static class VegetationInstanceIdUtility
    {
        public static ulong FromGlobalCell(int2 globalCell)
        {
            uint x = ZigZagEncode(globalCell.x);
            uint z = ZigZagEncode(globalCell.y);

            return ((ulong)x << 32) | z;
        }

        private static uint ZigZagEncode(int value)
        {
            uint bits = (uint)value;
            uint sign = (uint)(value >> 31);

            return (bits << 1) ^ sign;
        }
    }
}