namespace _Project.Features.ProceduralWorld.Domain.Chunks
{
    public readonly struct ChunkGenerationContext
    {
        public readonly ChunkCoordinate Coordinate;

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