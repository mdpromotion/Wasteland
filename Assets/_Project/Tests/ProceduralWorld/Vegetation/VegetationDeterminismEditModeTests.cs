using System.Collections.Generic;
using _Project.Features.ProceduralWorld.Domain.Vegetation;
using _Project.Features.ProceduralWorld.Infrastructure.Jobs.Vegetation;
using _Project.Features.ProceduralWorld.Infrastructure.Vegetation;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace _Project.Tests.ProceduralWorld.Vegetation
{
    public class VegetationDeterminismEditModeTests
    {
        [Test]
        public void VegetationGeneration_IsDeterministic()
        {
            const int resolution = 32;
            const int seed = 123456;

            int2 chunkCoordinate = new int2(17, -11);

            VegetationGenerationParams parameters = new VegetationGenerationParams
            {
                Priority = 0,
                PatchNoiseFrequency = 0.015f,
                PatchNoiseOctaves = 4,
                Coverage = 1f,
                EdgeSmoothing = 0.25f,
                Density = 1f,
                MinSlopeAngle = 0f,
                MaxSlopeAngle = 100f,
                MinScale = 0.8f,
                MaxScale = 1.2f,
                OccupancyRadius = 1.5f
            };

            List<VegetationInstanceData> first = Generate(
                resolution,
                chunkCoordinate,
                seed,
                parameters);

            List<VegetationInstanceData> second = Generate(
                resolution,
                chunkCoordinate,
                seed,
                parameters);

            try
            {
                Assert.That(second.Count, Is.EqualTo(first.Count));

                for (int i = 0; i < first.Count; i++)
                {
                    Assert.That(second[i].Id, Is.EqualTo(first[i].Id));
                    Assert.That(second[i].Position, Is.EqualTo(first[i].Position));
                    Assert.That(second[i].Rotation, Is.EqualTo(first[i].Rotation));
                    Assert.That(second[i].Scale, Is.EqualTo(first[i].Scale));
                }
            }
            finally
            {
                first.Clear();
                second.Clear();
            }
        }

        [Test]
        public void VegetationGeneration_IsDeterministicAcrossMultipleRuns()
        {
            const int resolution = 32;
            const int seed = 987654;

            int2 chunkCoordinate = new int2(-7, 23);

            VegetationGenerationParams parameters = new VegetationGenerationParams
            {
                Priority = 0,
                PatchNoiseFrequency = 0.02f,
                PatchNoiseOctaves = 5,
                Coverage = 1f,
                EdgeSmoothing = 0.5f,
                Density = 1f,
                MinSlopeAngle = 0f,
                MaxSlopeAngle = 100f,
                MinScale = 0.75f,
                MaxScale = 1.25f,
                OccupancyRadius = 1.5f
            };

            List<VegetationInstanceData> baseline = Generate(
                resolution,
                chunkCoordinate,
                seed,
                parameters);

            try
            {
                for (int run = 0; run < 20; run++)
                {
                    List<VegetationInstanceData> result = Generate(
                        resolution,
                        chunkCoordinate,
                        seed,
                        parameters);

                    try
                    {
                        Assert.That(
                            result.Count,
                            Is.EqualTo(baseline.Count),
                            $"Count mismatch on run {run}");

                        for (int i = 0; i < baseline.Count; i++)
                        {
                            Assert.That(
                                result[i].Id,
                                Is.EqualTo(baseline[i].Id),
                                $"Id mismatch on run {run}, index {i}");

                            Assert.That(
                                result[i].Position,
                                Is.EqualTo(baseline[i].Position),
                                $"Position mismatch on run {run}, index {i}");

                            Assert.That(
                                result[i].Rotation,
                                Is.EqualTo(baseline[i].Rotation),
                                $"Rotation mismatch on run {run}, index {i}");

                            Assert.That(
                                result[i].Scale,
                                Is.EqualTo(baseline[i].Scale),
                                $"Scale mismatch on run {run}, index {i}");
                        }
                    }
                    finally
                    {
                        result.Clear();
                    }
                }
            }
            finally
            {
                baseline.Clear();
            }
        }

        [Test]
        public void VegetationInstanceId_IsUniqueForDifferentGlobalCells()
        {
            var ids = new HashSet<ulong>();

            int2[] cells =
            {
                new int2(0, 0),
                new int2(1, 0),
                new int2(0, 1),
                new int2(-1, 0),
                new int2(0, -1),
                new int2(-100, 250),
                new int2(250, -100),
                new int2(int.MinValue, int.MaxValue),
                new int2(int.MaxValue, int.MinValue)
            };

            foreach (int2 cell in cells)
            {
                ulong id = VegetationInstanceIdUtility.FromGlobalCell(cell);

                Assert.That(
                    ids.Add(id),
                    Is.True,
                    $"Unexpected ID collision for cell {cell}");
            }
        }

        [Test]
        public void VegetationGeneration_ExtremeCoordinates_MaintainsPrecision()
        {
            const int resolution = 32;
            const int seed = 424242;
            
            int maxSafeChunk = (int.MaxValue / resolution) - 2;
            int2 extremeChunkCoordinate = new int2(maxSafeChunk, -maxSafeChunk);

            VegetationGenerationParams parameters = new VegetationGenerationParams
            {
                Priority = 0,
                PatchNoiseFrequency = 0.05f,
                PatchNoiseOctaves = 3,
                Coverage = 0.5f,
                EdgeSmoothing = 0.1f,
                Density = 0.7f,
                MinSlopeAngle = 0f,
                MaxSlopeAngle = 100f,
                MinScale = 1f,
                MaxScale = 2f,
                OccupancyRadius = 1f
            };

            List<VegetationInstanceData> firstRun = Generate(
                resolution,
                extremeChunkCoordinate,
                seed,
                parameters);

            List<VegetationInstanceData> secondRun = Generate(
                resolution,
                extremeChunkCoordinate,
                seed,
                parameters);

            try
            {
                Assert.That(secondRun.Count, Is.EqualTo(firstRun.Count), "The number of instances does not match.");

                for (int i = 0; i < firstRun.Count; i++)
                {
                    Assert.That(secondRun[i].Id, Is.EqualTo(firstRun[i].Id));
                    Assert.That(secondRun[i].Position, Is.EqualTo(firstRun[i].Position));
                }
                
                int cellCount = resolution * resolution;
                Assert.That(firstRun.Count, Is.GreaterThan(0), 
                    "The generator did not create any instances. Noise may have collapsed due to precision loss.");
                
                Assert.That(firstRun.Count, Is.LessThan(cellCount), 
                    "The generator populated every cell. Noise is not differentiating between cells (precision loss).");
                
                var uniquePositions = new HashSet<float3>();
                foreach (var instance in firstRun)
                {
                    bool isUnique = uniquePositions.Add(instance.Position);
                    Assert.That(isUnique, Is.True, "Multiple vegetation instances have the same local position. Cell hashes may have collapsed.");
                }
            }
            finally
            {
                firstRun.Clear();
                secondRun.Clear();
            }
        }

        private static List<VegetationInstanceData> Generate(
            int resolution,
            int2 chunkCoordinate,
            int seed,
            VegetationGenerationParams parameters)
        {
            int cellCount = resolution * resolution;

            NativeArray<float> heights = new NativeArray<float>(
                cellCount,
                Allocator.TempJob);

            NativeArray<float> waterSurfaceHeight = new NativeArray<float>(
                cellCount,
                Allocator.TempJob);

            NativeArray<float> riverMask = new NativeArray<float>(
                cellCount,
                Allocator.TempJob);

            NativeArray<byte> occupancy = new NativeArray<byte>(
                cellCount,
                Allocator.TempJob);

            NativeArray<VegetationInstanceData> candidates = new NativeArray<VegetationInstanceData>(
                cellCount,
                Allocator.TempJob);

            NativeArray<byte> candidateMask = new NativeArray<byte>(
                cellCount,
                Allocator.TempJob);

            NativeList<VegetationInstanceData> accepted = new NativeList<VegetationInstanceData>(
                cellCount,
                Allocator.TempJob);

            try
            {
                for (int i = 0; i < cellCount; i++)
                {
                    heights[i] = 0f;
                    waterSurfaceHeight[i] = -1000f;
                    riverMask[i] = 0f;
                }

                VegetationCandidateJob candidateJob = new VegetationCandidateJob(
                    resolution,
                    12,
                    chunkCoordinate,
                    parameters,
                    heights,
                    resolution,
                    waterSurfaceHeight,
                    riverMask,
                    seed,
                    candidates,
                    candidateMask);

                JobHandle stageA = candidateJob.Schedule(cellCount, 64);
                stageA.Complete();

                VegetationCommitJob commitJob = new VegetationCommitJob(
                    resolution,
                    parameters.OccupancyRadius,
                    occupancy,
                    candidates,
                    candidateMask,
                    accepted);

                JobHandle stageB = commitJob.Schedule(stageA);
                stageB.Complete();

                var result = new List<VegetationInstanceData>(accepted.Length);

                for (int i = 0; i < accepted.Length; i++)
                    result.Add(accepted[i]);

                accepted.Dispose();
                candidates.Dispose();
                candidateMask.Dispose();
                occupancy.Dispose();
                heights.Dispose();
                waterSurfaceHeight.Dispose();
                riverMask.Dispose();

                return result;
            }
            catch
            {
                if (accepted.IsCreated) accepted.Dispose();
                if (candidates.IsCreated) candidates.Dispose();
                if (candidateMask.IsCreated) candidateMask.Dispose();
                if (occupancy.IsCreated) occupancy.Dispose();
                if (heights.IsCreated) heights.Dispose();
                if (waterSurfaceHeight.IsCreated) waterSurfaceHeight.Dispose();
                if (riverMask.IsCreated) riverMask.Dispose();
                throw;
            }
        }
    }
}