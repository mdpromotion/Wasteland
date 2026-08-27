using System.Collections.Generic;
using _Project.Features.Core.Persistence.Regions;
using _Project.Features.ProceduralWorld.Domain.Persistence;
using NUnit.Framework;

namespace _Project.Tests.Core.Persistence
{
    public class ChunkDeltaSerializerTests
    {
        private ChunkDeltaSerializer _serializer;

        [SetUp]
        public void SetUp() => _serializer = new ChunkDeltaSerializer();

        [Test]
        public void RoundTrip_PreservesAllFields()
        {
            var deltas = new List<VegetationInstanceDelta>
            {
                new VegetationInstanceDelta(123UL, DeltaAction.Removed, null),
                new VegetationInstanceDelta(456UL, DeltaAction.Modified, new byte[] { 1, 2, 3 }),
            };
            var original = new ChunkDelta(new GeneratorVersionStamp(7), deltas);

            var roundTripped = _serializer.Deserialize(_serializer.Serialize(original));

            Assert.AreEqual(7, roundTripped.Versions.VegetationVersion);
            Assert.AreEqual(2, roundTripped.VegetationDeltas.Count);
            Assert.AreEqual(123UL, roundTripped.VegetationDeltas[0].Id);
            Assert.IsNull(roundTripped.VegetationDeltas[0].ExtraData);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, roundTripped.VegetationDeltas[1].ExtraData);
        }

        [Test]
        public void RoundTrip_EmptyDelta_Works()
        {
            var roundTripped = _serializer.Deserialize(_serializer.Serialize(ChunkDelta.Empty));
            Assert.AreEqual(0, roundTripped.VegetationDeltas.Count);
        }

        [Test]
        public void RoundTrip_LargeUlongId_PreservesFullRange()
        {
            var deltas = new List<VegetationInstanceDelta> { new VegetationInstanceDelta(ulong.MaxValue, DeltaAction.Removed, null) };
            var original = new ChunkDelta(new GeneratorVersionStamp(1), deltas);

            var roundTripped = _serializer.Deserialize(_serializer.Serialize(original));

            Assert.AreEqual(ulong.MaxValue, roundTripped.VegetationDeltas[0].Id);
        }

        [Test]
        public void RoundTrip_AllDeltaActions_PreserveCorrectEnum()
        {
            var deltas = new List<VegetationInstanceDelta>
            {
                new VegetationInstanceDelta(1UL, DeltaAction.Removed, null),
                new VegetationInstanceDelta(2UL, DeltaAction.Modified, null),
                new VegetationInstanceDelta(3UL, DeltaAction.Added, null),
            };
            var roundTripped = _serializer.Deserialize(_serializer.Serialize(new ChunkDelta(default, deltas)));

            Assert.AreEqual(DeltaAction.Removed, roundTripped.VegetationDeltas[0].Action);
            Assert.AreEqual(DeltaAction.Modified, roundTripped.VegetationDeltas[1].Action);
            Assert.AreEqual(DeltaAction.Added, roundTripped.VegetationDeltas[2].Action);
        }
    }
}