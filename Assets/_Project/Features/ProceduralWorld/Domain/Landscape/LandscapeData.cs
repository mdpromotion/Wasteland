using _Project.Features.ProceduralWorld.Domain.Chunks;
using Unity.Collections;

namespace _Project.Features.ProceduralWorld.Domain.Landscape
{
    /// <summary>
    /// Native terrain height data generated for a single chunk.
    /// </summary>
    /// <remarks>
    /// The height samples are stored in a flat NativeArray and are later converted
    /// to Unity Terrain data by the infrastructure layer.
    /// </remarks>
    public sealed class LandscapeData
    {
        public ChunkCoordinate Coordinate { get; }

        public NativeArray<float> Heights { get; }

        public int Resolution { get; }

        public LandscapeData(
            ChunkCoordinate coordinate,
            NativeArray<float> heights,
            int resolution)
        {
            Coordinate = coordinate;
            Heights = heights;
            Resolution = resolution;
        }


        public void Dispose()
        {
            if (Heights is { IsCreated: true, Length: > 0 })
            {
                Heights.Dispose();
            }
        }
    }
}