    using System;
    using System.Collections.Generic;
    using _Project.Features.ProceduralWorld.Domain.World;
    using _Project.Features.ProceduralWorld.Infrastructure.Jobs.Landscape;
    using Unity.Collections;
    using Unity.Mathematics;
    using Random = Unity.Mathematics.Random;

    namespace _Project.Features.ProceduralWorld.Infrastructure.Landscape
    {
        public sealed class TerrainNoiseSettingsProvider :
            IDisposable
        {
            private readonly IWorldSettings _worldSettings;

            private readonly Dictionary<int, NativeArray<float2>> _octaveOffsetsCache = new();

            public int Octaves => _worldSettings.Octaves;



            public TerrainNoiseSettingsProvider(
                IWorldSettings worldSettings)
            {
                _worldSettings = worldSettings;
            }



            public TerrainNoiseSettings Create()
            {
                return new TerrainNoiseSettings
                {
                    Scale = _worldSettings.Scale,
                    Octaves = _worldSettings.Octaves,
                    Persistence = _worldSettings.Persistence,
                    Lacunarity = _worldSettings.Lacunarity,
                    RedistributionPower = _worldSettings.RedistributionPower,
                    Offset = float2.zero
                };
            }



            public NativeArray<float2> GetOctaveOffsets(
                int octaves)
            {
                if (_octaveOffsetsCache.TryGetValue(octaves, out NativeArray<float2> offsets))
                {
                    return offsets;
                }

                offsets = new NativeArray<float2>(octaves, Allocator.Persistent);

                Random random = new Random((uint)math.max(_worldSettings.Seed, 1));

                for (int i = 0; i < octaves; i++)
                {
                    offsets[i] = new float2(
                        random.NextFloat(-100000f, 100000f),
                        random.NextFloat(-100000f, 100000f));
                }

                _octaveOffsetsCache.Add(octaves, offsets);

                return offsets;
            }



            public void Dispose()
            {
                foreach (NativeArray<float2> offsets in _octaveOffsetsCache.Values)
                {
                    if (offsets.IsCreated)
                        offsets.Dispose();
                }

                _octaveOffsetsCache.Clear();
            }
        }
    }