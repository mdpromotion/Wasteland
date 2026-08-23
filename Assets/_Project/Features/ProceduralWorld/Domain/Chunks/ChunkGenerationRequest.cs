namespace _Project.Features.ProceduralWorld.Domain.Chunks
{
    public readonly struct ChunkGenerationRequest
    {
        public readonly ChunkCoordinate Coordinate;

        public readonly int Resolution;
        
        public ChunkGenerationRequest(ChunkCoordinate coordinate, int resolution)
        {
            Coordinate = coordinate;
            Resolution = resolution;
        }
    }
}