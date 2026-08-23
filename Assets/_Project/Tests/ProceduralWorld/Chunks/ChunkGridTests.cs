using _Project.Features.ProceduralWorld.Domain.Chunks;
using NUnit.Framework;
using UnityEngine;

namespace _Project.Tests.ProceduralWorld.Chunks
{
    public sealed class ChunkGridTests
    {
        private ChunkGrid _grid;

        [SetUp]
        public void SetUp()
        {
            _grid = new ChunkGrid(100f, 50f);
        }

        [Test]
        public void ToWorldOffset_ConvertsCoordinateRelativeToOrigin()
        {
            Vector2 offset =
                _grid.ToWorldOffset(new ChunkCoordinate(2, 3));

            Assert.That(offset.x, Is.EqualTo(200f));
            Assert.That(offset.y, Is.EqualTo(150f));
        }

        [Test]
        public void ToWorldOffset_UsesChangedOrigin()
        {
            _grid.SetOriginCoordinate(new ChunkCoordinate(5, 10));

            Vector2 offset =
                _grid.ToWorldOffset(new ChunkCoordinate(7, 13));

            Assert.That(offset.x, Is.EqualTo(200f));
            Assert.That(offset.y, Is.EqualTo(150f));
        }

        [Test]
        public void ToChunkCoordinate_ConvertsWorldPosition()
        {
            ChunkCoordinate coordinate =
                _grid.ToChunkCoordinate(new Vector3(250f, 0f, 120f));

            Assert.That(coordinate.X, Is.EqualTo(2));
            Assert.That(coordinate.Y, Is.EqualTo(2));
        }

        [Test]
        public void ToChunkCoordinate_UsesFloorForNegativePositions()
        {
            ChunkCoordinate coordinate =
                _grid.ToChunkCoordinate(new Vector3(-0.1f, 0f, -50.1f));

            Assert.That(coordinate.X, Is.EqualTo(-1));
            Assert.That(coordinate.Y, Is.EqualTo(-2));
        }

        [Test]
        public void ToChunkCoordinate_AccountsForOrigin()
        {
            _grid.SetOriginCoordinate(new ChunkCoordinate(10, 20));

            ChunkCoordinate coordinate =
                _grid.ToChunkCoordinate(new Vector3(150f, 0f, 60f));

            Assert.That(coordinate.X, Is.EqualTo(11));
            Assert.That(coordinate.Y, Is.EqualTo(21));
        }
    }
}