using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Infrastructure.Chunks;
using _Project.Features.ProceduralWorld.Presentation.World;
using NUnit.Framework;
using UnityEngine;

namespace _Project.Tests.ProceduralWorld.World
{
    public sealed class WorldRebaseApplierTests
    {
        [Test]
        public void MoveChunkTo_MovesTerrainByDelta()
        {
            GameObject gameObject = new GameObject();
            Terrain terrain = gameObject.AddComponent<Terrain>();

            terrain.transform.position =
                new Vector3(100f, 20f, 300f);

            var chunk = new ChunkInstance(
                new ChunkCoordinate(0, 0),
                null,
                null,
                terrain);

            var applier = new WorldRebaseApplier();

            Vector3 delta =
                new Vector3(-50f, 0f, -125f);

            applier.MoveChunkTo(chunk, delta);

            Assert.That(
                terrain.transform.position,
                Is.EqualTo(
                    new Vector3(50f, 20f, 175f)));

            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void MoveChunkTo_WithoutTerrain_DoesNotThrow()
        {
            var chunk = new ChunkInstance(
                new ChunkCoordinate(0, 0),
                null,
                null,
                null);

            var applier = new WorldRebaseApplier();

            Assert.DoesNotThrow(() =>
                applier.MoveChunkTo(
                    chunk,
                    new Vector3(100f, 0f, 100f)));
        }
    }
}