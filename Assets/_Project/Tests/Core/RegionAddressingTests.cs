using _Project.Features.Persistence.Application;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using NUnit.Framework;

namespace _Project.Tests.Core.Persistence
{
    public class RegionAddressingTests
    {
        [TestCase(0, 0, 0, 0, 0)]
        [TestCase(15, 15, 0, 0, 255)]
        [TestCase(16, 0, 1, 0, 0)]
        [TestCase(-1, 0, -1, 0, 15)]
        [TestCase(-16, 0, -1, 0, 0)]
        [TestCase(-17, 0, -2, 0, 15)]
        public void ToSlot_MapsChunkCoordinateCorrectly(
            int chunkX, int chunkZ, int expectedRegionX, int expectedRegionZ, int expectedSlot)
        {
            var (regionX, regionZ, slot) = RegionAddressing.ToSlot(new ChunkCoordinate(chunkX, chunkZ));

            Assert.AreEqual(expectedRegionX, regionX);
            Assert.AreEqual(expectedRegionZ, regionZ);
            Assert.AreEqual(expectedSlot, slot);
        }

        [Test]
        public void ToSlot_IsBijective_WithinRegion()
        {
            var seen = new System.Collections.Generic.HashSet<int>();

            for (int x = 0; x < 16; x++)
            for (int z = 0; z < 16; z++)
            {
                var (_, _, slot) = RegionAddressing.ToSlot(new ChunkCoordinate(x, z));
                Assert.IsTrue(seen.Add(slot));
            }

            Assert.AreEqual(256, seen.Count);
        }
    }
}