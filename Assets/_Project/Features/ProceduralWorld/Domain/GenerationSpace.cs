using Unity.Mathematics;
using _Project.Features.ProceduralWorld.Domain.Chunks;

namespace _Project.Features.ProceduralWorld.Domain
{
    public static class GenerationSpace
    {
        public static double2 AbsoluteChunkOrigin( ChunkCoordinate coordinate, double chunkSizeX, double chunkSizeZ)
        {
            return new double2(coordinate.X * chunkSizeX, coordinate.Y * chunkSizeZ);
        }

        public static float2 LocalOffset(double2 absolutePosition, double2 zoneOrigin)
        {
            return new float2((float)(absolutePosition.x - zoneOrigin.x), (float)(absolutePosition.y - zoneOrigin.y));
        }
    }
}