namespace _Project.Features.ProceduralWorld.Domain.Chunks
{
    /// <summary>
    /// Describes a request to generate a single procedural-world chunk.
    /// </summary>
    /// <remarks>
    /// The request contains only the parameters required to initialize the
    /// <see cref="ChunkGenerationContext"/> used by the generation pipeline.
    /// </remarks>
    public readonly struct ChunkGenerationRequest
    {
        /// <summary>
        /// Logical coordinate of the chunk to generate.
        /// </summary>
        public readonly ChunkCoordinate Coordinate;

        /// <summary>
        /// Resolution requested for generated chunk data.
        /// </summary>
        public readonly int Resolution;
        
        public ChunkGenerationRequest(ChunkCoordinate coordinate, int resolution)
        {
            Coordinate = coordinate;
            Resolution = resolution;
        }
    }
}