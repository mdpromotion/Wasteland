using System.Collections.Generic;
using System.Reflection;
using _Project.Features.Items.Domain;
using _Project.Features.Items.Infrastructure;
using _Project.Features.Items.Presentation;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Tests.Items
{
    public class HeldItemVisualFactoryTests
    {
        private ItemDefinitionAsset _definition;

        [SetUp]
        public void SetUp()
        {
            _definition = ScriptableObject.CreateInstance<ItemDefinitionAsset>();
            SetField(_definition, "technicalId", "test_item");
            SetField(_definition, "displayName", "Test Item");
        }

        [TearDown]
        public void TearDown()
        {
            if (_definition != null)
                Object.DestroyImmediate(_definition);
        }

        [Test]
        public void Acquire_WithoutPrefabs_CreatesCubeFallbackWithoutPhysics()
        {
            var factory = new HeldItemVisualFactory();

            GameObject visual = factory.Acquire(_definition, null);

            try
            {
                Assert.IsNotNull(visual.GetComponent<Renderer>());
                Assert.IsNull(visual.GetComponent<Collider>());
            }
            finally
            {
                Object.DestroyImmediate(visual);
            }
        }

        [Test]
        public void Acquire_WithConfigVisualPrefab_UsesPrefabInsteadOfWorldPrefab()
        {
            GameObject prefabSource = new GameObject("Config Prefab");
            prefabSource.AddComponent<ConfigVisualMarker>();
            var config = CreateConfig(prefabSource);
            try
            {
                var factory = new HeldItemVisualFactory();
                GameObject visual = factory.Acquire(_definition, config);

                // The factory renames the instance to "Held <display name>", so
                // the source is proven by the marker component cloned into it —
                // a fallback cube or world visual would not carry the marker.
                Assert.AreEqual("Held Test Item", visual.name);
                Assert.IsNotNull(visual.GetComponent<ConfigVisualMarker>());
            }
            finally
            {
                Object.DestroyImmediate(prefabSource);
                Object.DestroyImmediate(config);
            }
        }

        private sealed class ConfigVisualMarker : MonoBehaviour
        {
        }

        [Test]
        public void Acquire_SameDefinitionTwice_ReturnsSameInstance()
        {
            var factory = new HeldItemVisualFactory();

            GameObject first = factory.Acquire(_definition, null);
            GameObject second = factory.Acquire(_definition, null);

            try
            {
                Assert.AreSame(first, second);
                Assert.IsTrue(first.activeSelf);
            }
            finally
            {
                Object.DestroyImmediate(first);
            }
        }

        [Test]
        public void Acquire_DifferentDefinitions_ReturnDistinctInstances()
        {
            var other = ScriptableObject.CreateInstance<ItemDefinitionAsset>();
            SetField(other, "technicalId", "other_item");
            var factory = new HeldItemVisualFactory();

            try
            {
                GameObject first = factory.Acquire(_definition, null);
                GameObject second = factory.Acquire(other, null);

                Assert.AreNotSame(first, second);
            }
            finally
            {
                Object.DestroyImmediate(other);
                DisposeFactory(factory);
            }
        }

        [Test]
        public void Acquire_AfterRelease_ReactivatesCachedInstance()
        {
            var factory = new HeldItemVisualFactory();
            GameObject first = factory.Acquire(_definition, null);

            factory.Release(new ItemId("test_item"));

            GameObject second = factory.Acquire(_definition, null);
            try
            {
                Assert.AreSame(first, second);
                Assert.IsTrue(second.activeSelf);
            }
            finally
            {
                DisposeFactory(factory);
            }
        }

        [Test]
        public void Release_HidesInstanceButKeepsItCached()
        {
            var factory = new HeldItemVisualFactory();
            GameObject visual = factory.Acquire(_definition, null);

            factory.Release(new ItemId("test_item"));

            Assert.IsNotNull(visual);
            Assert.IsFalse(visual.activeSelf);
        }

        [Test]
        public void Release_UnknownItem_DoesNotThrow()
        {
            var factory = new HeldItemVisualFactory();

            Assert.DoesNotThrow(() => factory.Release(new ItemId("unknown_item")));
        }

        [Test]
        public void Acquire_WithConfigVisualPrefab_StripsWorldBehaviourAndResetsPivot()
        {
            GameObject source = CreatePrefabSource();
            var config = CreateConfig(source);
            try
            {
                var factory = new HeldItemVisualFactory();

                GameObject visual = factory.Acquire(_definition, config);

                try
                {
                    Assert.AreNotSame(source, visual);
                    Assert.IsNull(visual.GetComponent<Rigidbody>());
                    Assert.IsNull(visual.GetComponent<Collider>());
                    Assert.AreEqual(Vector3.zero, visual.transform.localPosition);
                    Assert.AreEqual(Quaternion.identity, visual.transform.localRotation);
                }
                finally
                {
                    DisposeFactory(factory);
                }
            }
            finally
            {
                Object.DestroyImmediate(source);
                Object.DestroyImmediate(config);
            }
        }

        private static GameObject CreatePrefabSource()
        {
            GameObject source = new GameObject("Prefab Source");
            source.transform.localPosition = new Vector3(1f, 2f, 3f);
            source.transform.localRotation = Quaternion.Euler(10f, 20f, 30f);
            source.AddComponent<Rigidbody>();
            source.AddComponent<BoxCollider>();
            return source;
        }

        private static HeldItemConfig CreateConfig(GameObject visualPrefab)
        {
            HeldItemConfig config = ScriptableObject.CreateInstance<HeldItemConfig>();
            SetField(config, "visualPrefab", visualPrefab);
            return config;
        }

        private static void SetField(ScriptableObject target, string fieldName, object value)
        {
            FieldInfo field = target.GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' is not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private static void DisposeFactory(IHeldItemVisualFactory factory)
        {
            factory.Dispose();
        }
    }
}
