using Unity.Mathematics;
using _Project.Features.ProceduralWorld.Domain.Chunks;

namespace _Project.Features.ProceduralWorld.Domain
{
    /// <summary>
    /// Provides coordinate transformations used by the procedural world generation pipeline.
    /// <para>
    /// Absolute positions use double precision to preserve accuracy in large worlds,
    /// while local offsets use float precision for chunk- and zone-relative calculations.
    /// </para>
    /// </summary>
    public static class GenerationSpace
    {
        /// <summary>
        /// Calculates the absolute world-space origin of a chunk.
        /// </summary>
        /// <param name="coordinate">
        /// Logical chunk coordinate. X identifies the chunk along the world X axis;
        /// Y identifies the chunk along the world Z axis.
        /// </param>
        /// <param name="chunkSizeX">Chunk size along the world X axis.</param>
        /// <param name="chunkSizeZ">Chunk size along the world Z axis.</param>
        /// <returns>
        /// The absolute world-space X/Z position of the chunk origin.
        /// </returns>
        public static double2 AbsoluteChunkOrigin(
            ChunkCoordinate coordinate,
            double chunkSizeX,
            double chunkSizeZ)
        {
            return new double2(
                coordinate.X * chunkSizeX,
                coordinate.Y * chunkSizeZ);
        }

        /// <summary>
        /// Converts an absolute world-space position to an offset relative to a zone origin.
        /// </summary>
        /// <param name="absolutePosition">
        /// Absolute world-space X/Z position.
        /// </param>
        /// <param name="zoneOrigin">
        /// Absolute world-space X/Z origin of the zone.
        /// </param>
        /// <returns>
        /// A float-precision X/Z offset relative to <paramref name="zoneOrigin"/>.
        /// </returns>
        public static float2 LocalOffset(
            double2 absolutePosition,
            double2 zoneOrigin)
        {
            return new float2(
                (float)(absolutePosition.x - zoneOrigin.x),
                (float)(absolutePosition.y - zoneOrigin.y));
        }
    }
}