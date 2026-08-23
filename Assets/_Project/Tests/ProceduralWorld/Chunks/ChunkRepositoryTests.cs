using System.Linq;
using _Project.Features.ProceduralWorld.Application.Chunks;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Infrastructure.Chunks;
using NUnit.Framework;

namespace _Project.Tests.ProceduralWorld.Chunks
{
    public sealed class ChunkRepositoryTests
    {
        private ChunkRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _repository = new ChunkRepository();
        }

        [TearDown]
        public void TearDown()
        {
            _repository.Dispose();
        }

        [Test]
        public void Add_ThenContains_ReturnsTrue()
        {
            ChunkCoordinate coordinate = new ChunkCoordinate(10, 20);
            ChunkInstance chunk = CreateChunk(coordinate);

            _repository.Add(chunk);

            Assert.That(_repository.Contains(coordinate), Is.True);
            Assert.That(_repository.Get(coordinate), Is.SameAs(chunk));
        }

        [Test]
        public void TryGet_WhenChunkExists_ReturnsChunk()
        {
            ChunkCoordinate coordinate = new ChunkCoordinate(3, 7);
            ChunkInstance chunk = CreateChunk(coordinate);

            _repository.Add(chunk);

            bool result = _repository.TryGet(coordinate, out ChunkInstance actual);

            Assert.That(result, Is.True);
            Assert.That(actual, Is.SameAs(chunk));
        }

        [Test]
        public void Get_WhenChunkDoesNotExist_ReturnsNull()
        {
            ChunkInstance result =
                _repository.Get(new ChunkCoordinate(100, 100));

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Remove_ExistingChunk_RemovesIt()
        {
            ChunkCoordinate coordinate = new ChunkCoordinate(1, 2);
            ChunkInstance chunk = CreateChunk(coordinate);

            _repository.Add(chunk);
            _repository.Remove(coordinate);

            Assert.That(_repository.Contains(coordinate), Is.False);
            Assert.That(_repository.Get(coordinate), Is.Null);
        }

        [Test]
        public void All_ReturnsAllStoredChunks()
        {
            ChunkInstance first = CreateChunk(new ChunkCoordinate(0, 0));
            ChunkInstance second = CreateChunk(new ChunkCoordinate(1, 0));

            _repository.Add(first);
            _repository.Add(second);

            Assert.That(_repository.All.Count(), Is.EqualTo(2));
            Assert.That(_repository.All, Does.Contain(first));
            Assert.That(_repository.All, Does.Contain(second));
        }

        private static ChunkInstance CreateChunk(ChunkCoordinate coordinate)
        {
            return new ChunkInstance(
                coordinate,
                null,
                null,
                null);
        }
    }
}