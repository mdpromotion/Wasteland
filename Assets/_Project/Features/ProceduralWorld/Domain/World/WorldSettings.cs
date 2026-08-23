using UnityEngine;

namespace _Project.Features.ProceduralWorld.Domain.World
{
    public interface IWorldSettings
    {
        int Seed { get; }
        int Octaves { get; }
        float Scale { get; }
        float Persistence { get; }
        float Lacunarity { get; }
        float RedistributionPower { get; }
    }
    
    public interface IWorldSettingsController
    {
        void SetSeed(int seed);
    }
    
    public sealed class WorldSettings : IWorldSettings, IWorldSettingsController
    {
        public int Seed { get; private set; }

        public int Octaves { get; }
        public float Scale { get; }
        public float Persistence { get; }
        public float Lacunarity { get; }
        public float RedistributionPower { get; }

        public WorldSettings(WorldSettingsConfig config)
        {
            Octaves = config.Octaves;
            Scale = config.Scale;
            Persistence = config.Persistence;
            Lacunarity = config.Lacunarity;
            RedistributionPower = config.RedistributionPower;
        }

        public void SetSeed(int seed)
        {
            Seed = seed;
        }
    }
}
