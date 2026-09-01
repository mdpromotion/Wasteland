using _Project.Features.ProceduralWorld.Domain.Vegetation;
using _Project.Features.ProceduralWorld.Infrastructure.Vegetation;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace _Project.Features.ProceduralWorld.Infrastructure.Jobs.Vegetation
{
    [BurstCompile]
    public struct VegetationCandidateJob : IJobParallelFor
    {
        [ReadOnly] private readonly int _resolution;
        [ReadOnly] private readonly int2 _chunkCoordinate;
        [ReadOnly] private readonly VegetationGenerationParams _species;

        [ReadOnly] private readonly NativeArray<float> _heights;
        [ReadOnly] private readonly int _heightResolution;

        [ReadOnly] private readonly NativeArray<float> _waterSurfaceHeight;
        [ReadOnly] private readonly NativeArray<float> _riverMask;

        [ReadOnly] private readonly int _worldSeed;

        [WriteOnly] private NativeArray<VegetationInstanceData> _candidates;
        [WriteOnly] private NativeArray<byte> _candidateMask;
        
        [ReadOnly] private readonly float _cellSize;

        public VegetationCandidateJob(
            int resolution,
            float cellSize, 
            int2 chunkCoordinate,
            VegetationGenerationParams species,
            NativeArray<float> heights,
            int heightResolution,
            NativeArray<float> waterSurfaceHeight,
            NativeArray<float> riverMask,
            int worldSeed,
            NativeArray<VegetationInstanceData> candidates,
            NativeArray<byte> candidateMask)
        {
            _resolution = resolution;
            _cellSize = cellSize;
            _chunkCoordinate = chunkCoordinate;
            _species = species;
            _heights = heights;
            _heightResolution = heightResolution;
            _waterSurfaceHeight = waterSurfaceHeight;
            _riverMask = riverMask;
            _worldSeed = worldSeed;
            _candidates = candidates;
            _candidateMask = candidateMask;
        }

        public void Execute(int index)
        {
            _candidateMask[index] = 0;

            int localX = index % _resolution;
            int localZ = index / _resolution;
            
            double2 globalCell = new double2(_chunkCoordinate.x, _chunkCoordinate.y) * _resolution + new double2(localX, localZ);
            
            uint cellHash = math.hash(new double3(globalCell.x, globalCell.y, _worldSeed));
            Random random = Random.CreateFromIndex(cellHash);

            float rawSum = FractalNoiseSum(
                globalCell,
                _species.PatchNoiseFrequency,
                _species.PatchNoiseOctaves,
                _worldSeed,
                out float stdDev);

            float patchNoise = NormalCdf(rawSum / stdDev);

            float distanceToEdge = patchNoise - (1f - _species.Coverage);
            if (distanceToEdge < 0f)
                return;

            float edgeFactor = math.smoothstep(
                0f,
                math.max(_species.EdgeSmoothing, 0.0001f),
                distanceToEdge);

            float slope = SampleSlope(localX, localZ);
            float slopeFactor = SlopeFalloff(slope, _species.MinSlopeAngle, _species.MaxSlopeAngle);

            float finalProbability = _species.Density * edgeFactor * slopeFactor;
            if (random.NextFloat() > finalProbability)
                return;

            float height = _heights[localZ * _heightResolution + localX];

            int hydroIndex = localZ * _heightResolution + localX;
            float waterHeight = _waterSurfaceHeight[hydroIndex];
            float riverMaskValue = _riverMask[hydroIndex];

            const float riverMaskThreshold = 0.05f;

            bool isRiverPresent = riverMaskValue > riverMaskThreshold;
            bool isBelowOrAtWater = height <= waterHeight;

            if (isRiverPresent && isBelowOrAtWater)
                return;

            float scale = random.NextFloat(_species.MinScale, _species.MaxScale);
            float rotation = random.NextFloat(0f, math.PI * 2f);
            
            float offsetX = random.NextFloat(0f, 1f);
            float offsetZ = random.NextFloat(0f, 1f);
            
            float posX = localX + offsetX;
            float posZ = localZ + offsetZ;

            _candidates[index] = new VegetationInstanceData
            {
                Id = VegetationInstanceIdUtility.FromGlobalCell(new int2((int)globalCell.x, (int)globalCell.y)),
                Position = new float3(localX * _cellSize, height, localZ * _cellSize),
                Rotation = rotation,
                Scale = scale,
                IsBreakable = _species.IsBreakable
            };

            _candidateMask[index] = 1;
        }

        private const float SingleOctaveNoiseStd = 0.29f;
        
        private static float FractalNoiseSum(double2 cell, double frequency, int octaves, int seed, out float stdDev)
        {
            float value = 0f;
            float amplitude = 0.5f;
            float sumSquaredAmplitude = 0f;
            double freq = frequency;
            double2 offset = SeedToOffsetDouble(seed);

            for (int i = 0; i < octaves; i++)
            {
                double2 sampleCoord = cell * freq + offset;
                
                float2 floatCoord = ToNoiseFloat2(sampleCoord);
        
                value += noise.cnoise(floatCoord) * amplitude;
        
                sumSquaredAmplitude += amplitude * amplitude;
                freq *= 2.0;
                amplitude *= 0.5f;
            }

            stdDev = math.sqrt(sumSquaredAmplitude) * SingleOctaveNoiseStd;
            stdDev = math.max(stdDev, 0.0001f);

            return value;
        }

        private static float2 ToNoiseFloat2(double2 coord)
        {
            double2 floorCoord = math.floor(coord);
            double2 fracCoord = coord - floorCoord;
            
            double2 modFloor = floorCoord - math.floor(floorCoord / 289.0) * 289.0;
    
            return (float2)(modFloor + fracCoord);
        }
        
        private static double2 SeedToOffsetDouble(int seed)
        {
            uint h = math.hash(new int2(seed, seed * 7 + 13));
            Random rnd = Random.CreateFromIndex(h);
            return new double2(rnd.NextDouble(0.0, 1000.0), rnd.NextDouble(0.0, 1000.0));
        }

        private static float NormalCdf(float z)
        {
            float x = z / 1.41421356f;
            float t = 1f / (1f + 0.3275911f * math.abs(x));
            float poly = t * (0.254829592f + t * (-0.284496736f + t * (1.421413741f +
                         t * (-1.453152027f + t * 1.061405429f))));
            float erf = 1f - poly * math.exp(-x * x);
            erf = x >= 0f ? erf : -erf;

            return 0.5f * (1f + erf);
        }

        private float SampleSlope(int localX, int localZ)
        {
            int xMinus = math.max(localX - 1, 0);
            int xPlus = math.min(localX + 1, _heightResolution - 1);
            int zMinus = math.max(localZ - 1, 0);
            int zPlus = math.min(localZ + 1, _heightResolution - 1);

            float hL = _heights[localZ * _heightResolution + xMinus];
            float hR = _heights[localZ * _heightResolution + xPlus];
            float hD = _heights[zMinus * _heightResolution + localX];
            float hU = _heights[zPlus * _heightResolution + localX];

            float2 gradient = new float2(hR - hL, hU - hD);
            return math.length(gradient);
        }

        private static float SlopeFalloff(float slope, float minSlope, float maxSlope)
        {
            if (slope < minSlope) return 1f;
            if (slope >= maxSlope) return 0;
            return 1f - (slope - minSlope) / (maxSlope - minSlope);
        }
    }
}