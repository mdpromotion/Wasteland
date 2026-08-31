using UnityEngine;

namespace _Project.Features.ProceduralWorld.Domain.World
{
    /// <summary>
    /// Read-only procedural generation settings used by the world generation pipeline.
    /// </summary>
    public interface IWorldSettings
    {
        string Name { get; }
        int Seed { get; }
        int Octaves { get; }
        float Scale { get; }
        float Persistence { get; }
        float Lacunarity { get; }
        float RedistributionPower { get; }
    }
    
    /// <summary>
    /// Controls runtime-modifiable world settings.
    /// </summary>
    public interface IWorldSettingsController
    {
        void SetName(string value);
        void SetSeed(int seed);
    }
    
    /// <summary>
    /// Runtime representation of the procedural world generation settings.
    /// </summary>
    /// <remarks>
    /// Immutable generation parameters are copied from <see cref="WorldSettingsConfig"/>
    /// when the runtime settings object is created. The seed is the only setting exposed
    /// for runtime mutation through <see cref="IWorldSettingsController"/>.
    /// </remarks>
    public sealed class WorldSettings : IWorldSettings, IWorldSettingsController
    {
        public string Name { get; private set; }
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

        public void SetName(string value)
        {
            Name = value;
        }
        public void SetSeed(int seed)
        {
            Seed = seed;
        }
    }
}
