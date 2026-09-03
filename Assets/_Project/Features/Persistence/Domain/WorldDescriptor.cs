namespace _Project.Features.Persistence.Domain
{
    public sealed class WorldDescriptor
    {
        public string Name { get; }
        public int Seed { get; }
        public long CreatedAtTicks { get; }
        public float? CurrentTick { get; }

        public WorldDescriptor(string name, int seed, long createdAtTicks, float? currentTick = null)
        {
            Name = name;
            Seed = seed;
            CreatedAtTicks = createdAtTicks;
            CurrentTick = currentTick;
        }
    }
}