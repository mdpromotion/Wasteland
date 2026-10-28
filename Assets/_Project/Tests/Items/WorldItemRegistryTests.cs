using System.Collections.Generic;
using System.Linq;
using _Project.Features.Items.Infrastructure;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Tests.Items
{
    public class WorldItemRegistryTests
    {
        private readonly List<GameObject> _created = new();
        private WorldItemRegistry _registry;

        [SetUp]
        public void SetUp() => _registry = new WorldItemRegistry();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject gameObject in _created)
                Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void Register_NewId_TrackedAsLive()
        {
            WorldItemPickup pickup = CreatePickup();

            _registry.Register(1UL, pickup);

            Assert.IsTrue(_registry.IsLive(1UL));
            Assert.IsTrue(_registry.Live.Contains(pickup));
        }

        [Test]
        public void Register_DuplicateId_Throws()
        {
            _registry.Register(1UL, CreatePickup());

            Assert.Throws<System.InvalidOperationException>(
                () => _registry.Register(1UL, CreatePickup()));
        }

        [Test]
        public void Register_NullPickup_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => _registry.Register(1UL, null));
        }

        [Test]
        public void Release_OwnerMatches_RemovesEntry()
        {
            WorldItemPickup pickup = CreatePickup();
            _registry.Register(1UL, pickup);

            _registry.Release(1UL, pickup);

            Assert.IsFalse(_registry.IsLive(1UL));
        }

        [Test]
        public void Release_WithDifferentOwner_DoesNotEvictLiveEntry()
        {
            WorldItemPickup owner = CreatePickup();
            WorldItemPickup other = CreatePickup();
            _registry.Register(1UL, owner);

            _registry.Release(1UL, other);

            Assert.IsTrue(_registry.IsLive(1UL));
        }

        [Test]
        public void Release_UnknownId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _registry.Release(42UL, CreatePickup()));
        }

        private WorldItemPickup CreatePickup()
        {
            var gameObject = new GameObject("Test Pickup");
            _created.Add(gameObject);
            return gameObject.AddComponent<WorldItemPickup>();
        }
    }

    public class WorldItemRebaseSyncTests
    {
        private readonly List<GameObject> _created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject gameObject in _created)
                Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void OnWorldRebased_ShiftsEveryLiveEntityByDelta()
        {
            var registry = new WorldItemRegistry();
            WorldItemPickup a = CreatePickup("a");
            WorldItemPickup b = CreatePickup("b");
            a.transform.position = new Vector3(1f, 2f, 3f);
            b.transform.position = new Vector3(-4f, 0f, 0.5f);
            registry.Register(1UL, a);
            registry.Register(2UL, b);

            new WorldItemRebaseSync(registry).OnWorldRebased(new Vector3(-500f, 0f, 250f));

            Assert.AreEqual(new Vector3(-499f, 2f, 253f), a.transform.position);
            Assert.AreEqual(new Vector3(-504f, 0f, 250.5f), b.transform.position);
        }

        [Test]
        public void OnWorldRebased_DestroyedEntries_AreIgnored()
        {
            var registry = new WorldItemRegistry();
            WorldItemPickup alive = CreatePickup("alive");
            WorldItemPickup dead = CreatePickup("dead");
            registry.Register(1UL, alive);
            registry.Register(2UL, dead);

            Object.DestroyImmediate(dead.gameObject);

            Assert.DoesNotThrow(
                () => new WorldItemRebaseSync(registry).OnWorldRebased(new Vector3(10f, 0f, 0f)));

            Assert.AreEqual(new Vector3(10f, 0f, 0f), alive.transform.position);
        }

        [Test]
        public void OnWorldRebased_UnregisteredEntities_AreNotShifted()
        {
            WorldItemPickup outside = CreatePickup("outside");
            outside.transform.position = Vector3.zero;

            new WorldItemRebaseSync(new WorldItemRegistry()).OnWorldRebased(new Vector3(7f, 0f, 7f));

            Assert.AreEqual(Vector3.zero, outside.transform.position);
        }

        private WorldItemPickup CreatePickup(string name)
        {
            var gameObject = new GameObject(name);
            _created.Add(gameObject);
            return gameObject.AddComponent<WorldItemPickup>();
        }
    }
}
