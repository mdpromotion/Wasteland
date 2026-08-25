namespace _Project.Features.ProceduralWorld.Domain.Chunks
{
    /// <summary>
    /// Immutable context shared by generation stages for a single chunk.
    /// </summary>
    /// <remarks>
    /// The context identifies the target chunk and the resolution that all generation
    /// stages must use for their chunk-local data.
    /// </remarks>
    public readonly struct ChunkGenerationContext
    {
        /// <summary>
        /// Logical coordinate of the chunk being generated.
        /// </summary>
        public readonly ChunkCoordinate Coordinate;

        /// <summary>
        /// Resolution used by chunk generation stages.
        /// </summary>
        /// <remarks>
        /// Generation stages should use the same resolution when producing data that belongs
        /// to this chunk.
        /// </remarks>
        public readonly int Resolution;
        
        public ChunkGenerationContext(
            ChunkCoordinate coordinate,
            int resolution)
        {
            Coordinate = coordinate;
            Resolution = resolution;
        }
    }
}