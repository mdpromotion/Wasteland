using _Project.Features.ProceduralWorld.Domain.Chunks;
using NUnit.Framework;

namespace _Project.Tests.ProceduralWorld.Chunks
{
    public sealed class ChunkCoordinateDistanceComparerTests
    {
        [Test]
        public void Compare_CloserCoordinate_ComesFirst()
        {
            var comparer = new ChunkCoordinateDistanceComparer
            {
                Center = new ChunkCoordinate(0, 0)
            };

            int result = comparer.Compare(
                new ChunkCoordinate(1, 0),
                new ChunkCoordinate(3, 0));

            Assert.That(result, Is.LessThan(0));
        }

        [Test]
        public void Compare_FartherCoordinate_ComesAfter()
        {
            var comparer = new ChunkCoordinateDistanceComparer
            {
                Center = new ChunkCoordinate(0, 0)
            };

            int result = comparer.Compare(
                new ChunkCoordinate(3, 0),
                new ChunkCoordinate(1, 0));

            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Compare_SameDistance_ReturnsZero()
        {
            var comparer = new ChunkCoordinateDistanceComparer
            {
                Center = new ChunkCoordinate(0, 0)
            };

            int result = comparer.Compare(
                new ChunkCoordinate(1, 0),
                new ChunkCoordinate(0, 1));

            Assert.That(result, Is.EqualTo(0));
        }
    }
}