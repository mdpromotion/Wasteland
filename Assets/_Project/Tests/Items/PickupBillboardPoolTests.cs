using System.Collections.Generic;
using System.Reflection;
using _Project.Features.Items.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace _Project.Tests.Items
{
    public class PickupBillboardPoolTests
    {
        private readonly List<GameObject> _created = new();
        private HotbarHudSettings _settings;

        [SetUp]
        public void SetUp() => _settings = ScriptableObject.CreateInstance<HotbarHudSettings>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject gameObject in _created)
                if (gameObject != null)
                    Object.DestroyImmediate(gameObject);

            if (_settings != null)
                Object.DestroyImmediate(_settings);
        }

        [Test]
        public void Release_WhenPoolRootAlreadyDestroyed_DoesNotSpawnANewRoot()
        {
            // Regression guard: when the scene closes in an arbitrary component
            // order, a pickup's OnDestroy may release its billboard AFTER the
            // pool root was destroyed. Releasing must not resurrect the root —
            // that leaked "Pickup Billboard Pool" into the closing scene.
            PickupBillboardPool pool = new PickupBillboardPool(_settings);
            PooledBillboard billboard = CreateFakeBillboard();
            AddToInUse(pool, billboard);

            pool.Release(billboard);

            AssertNoPoolRootGameObject();
            Assert.IsFalse(billboard.Root.activeSelf);
        }

        [Test]
        public void Release_UnknownBillboard_DoesNotThrow()
        {
            PickupBillboardPool pool = new PickupBillboardPool(_settings);

            Assert.DoesNotThrow(() => pool.Release(CreateFakeBillboard()));
            AssertNoPoolRootGameObject();
        }

        [Test]
        public void Release_NullBillboard_DoesNotThrow()
        {
            PickupBillboardPool pool = new PickupBillboardPool(_settings);

            Assert.DoesNotThrow(() => pool.Release(null));
            AssertNoPoolRootGameObject();
        }

        private PooledBillboard CreateFakeBillboard()
        {
            var root = new GameObject("Fake Billboard Root");
            _created.Add(root);
            Canvas canvas = root.AddComponent<Canvas>();

            return new PooledBillboard(root, canvas, null, null, null, null);
        }

        private static void AddToInUse(PickupBillboardPool pool, PooledBillboard billboard)
        {
            var inUse = (List<PooledBillboard>)typeof(PickupBillboardPool)
                .GetField("_inUse", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(pool);
            inUse.Add(billboard);
        }

        private static void AssertNoPoolRootGameObject()
        {
            GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject gameObject in all)
                if (gameObject != null && gameObject.name == "Pickup Billboard Pool")
                    Assert.Fail("Pool root must not be created by Pool.Release.");
        }
    }
}
