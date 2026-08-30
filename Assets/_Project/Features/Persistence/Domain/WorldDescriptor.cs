namespace _Project.Features.Persistence.Domain
{
    public sealed class WorldDescriptor
    {
        public string Name { get; }
        public int Seed { get; }
        public long CreatedAtTicks { get; }

        public WorldDescriptor(string name, int seed, long createdAtTicks)
        {
            Name = name;
            Seed = seed;
            CreatedAtTicks = createdAtTicks;
        }
    }
}