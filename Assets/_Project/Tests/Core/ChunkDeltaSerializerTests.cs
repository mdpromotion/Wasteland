using System;
using System.Collections.Generic;
using System.IO;
using _Project.Features.Persistence.Infrastructure;
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

        private static byte[] SerializeLegacyPayload(GeneratorVersionStamp versions, List<VegetationInstanceDelta> deltas)
        {
            using var ms = new System.IO.MemoryStream();
            using var w = new System.IO.BinaryWriter(ms);

            w.Write(versions.VegetationVersion);
            w.Write(deltas.Count);

            foreach (var d in deltas)
            {
                w.Write(d.Id);
                w.Write((byte)d.Action);
                var extra = d.ExtraData ?? Array.Empty<byte>();
                w.Write(extra.Length);
                w.Write(extra);
            }

            return ms.ToArray();
        }

        [Test]
        public void RoundTrip_PreservesItemDeltas()
        {
            var items = new List<WorldItemDelta>
            {
                new WorldItemDelta(11UL, DeltaAction.Added, "iron_axe", 1.5f, 2.5f, 3.5f),
                new WorldItemDelta(99UL, DeltaAction.Removed, null, 0f, 0f, 0f),
            };
            var original = new ChunkDelta(
                new GeneratorVersionStamp(7),
                new List<VegetationInstanceDelta>(),
                items);

            var roundTripped = _serializer.Deserialize(_serializer.Serialize(original));

            Assert.AreEqual(2, roundTripped.ItemDeltas.Count);
            Assert.AreEqual(11UL, roundTripped.ItemDeltas[0].Id);
            Assert.AreEqual(DeltaAction.Added, roundTripped.ItemDeltas[0].Action);
            Assert.AreEqual("iron_axe", roundTripped.ItemDeltas[0].ItemKey);
            Assert.AreEqual(1.5f, roundTripped.ItemDeltas[0].X);
            Assert.AreEqual(2.5f, roundTripped.ItemDeltas[0].Y);
            Assert.AreEqual(3.5f, roundTripped.ItemDeltas[0].Z);
            Assert.AreEqual(99UL, roundTripped.ItemDeltas[1].Id);
            Assert.AreEqual(DeltaAction.Removed, roundTripped.ItemDeltas[1].Action);
        }

        [Test]
        public void RoundTrip_VegetationOnlyDelta_HasEmptyItemSection()
        {
            var original = new ChunkDelta(
                new GeneratorVersionStamp(3),
                new List<VegetationInstanceDelta>
                {
                    new VegetationInstanceDelta(123UL, DeltaAction.Removed, null),
                });

            var roundTripped = _serializer.Deserialize(_serializer.Serialize(original));

            Assert.AreEqual(1, roundTripped.VegetationDeltas.Count);
            Assert.AreEqual(0, roundTripped.ItemDeltas.Count);
        }

        [Test]
        public void Deserialize_LegacyPayloadWithoutItemSection_TreatsItemsAsEmpty()
        {
            var legacy = new List<VegetationInstanceDelta>
            {
                new VegetationInstanceDelta(77UL, DeltaAction.Modified, new byte[] { 4 }),
            };
            byte[] payload = SerializeLegacyPayload(new GeneratorVersionStamp(9), legacy);

            var roundTripped = _serializer.Deserialize(payload);

            Assert.AreEqual(9, roundTripped.Versions.VegetationVersion);
            Assert.AreEqual(1, roundTripped.VegetationDeltas.Count);
            Assert.AreEqual(77UL, roundTripped.VegetationDeltas[0].Id);
            Assert.AreEqual(0, roundTripped.ItemDeltas.Count);
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