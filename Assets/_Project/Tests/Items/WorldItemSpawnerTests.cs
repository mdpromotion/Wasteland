using System.Linq;
using _Project.Features.Items.Application;
using _Project.Features.Items.Domain;
using _Project.Features.Items.Infrastructure;
using _Project.Features.Items.Presentation;
using _Project.Features.ProceduralWorld.Application.Persistence;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Persistence;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Tests.Items
{
    public class WorldItemSpawnerTests
    {
        private static readonly string TestItemKey = "persistence_test_item";

        private ItemCatalogAsset _catalog;
        private ItemDefinitionAsset _definition;
        private ItemDropSettings _dropSettings;
        private HotbarHudSettings _hudSettings;
        private ChunkGrid _grid;
        private DirtyChunkRegistry _dirtyRegistry;
        private ChunkMutationTracker _tracker;
        private WorldItemRegistry _registry;
        private FakeItemCollector _collector;
        private WorldItemSpawner _spawner;

        [SetUp]
        public void SetUp()
        {
            _catalog = ScriptableObject.CreateInstance<ItemCatalogAsset>();
            _definition = ScriptableObject.CreateInstance<ItemDefinitionAsset>();
            _dropSettings = ScriptableObject.CreateInstance<ItemDropSettings>();
            _hudSettings = ScriptableObject.CreateInstance<HotbarHudSettings>();

            SetObjectField(_definition, "technicalId", TestItemKey);
            SetObjectField(_definition, "displayName", "Persistence Test Item");
            SetObjectField(_catalog, "items", new[] { _definition });

            // Asymmetric chunk sizes catch X/Z swap mistakes (100 x 50).
            _grid = new ChunkGrid(100f, 50f);
            _dirtyRegistry = new DirtyChunkRegistry();
            _tracker = new ChunkMutationTracker(_dirtyRegistry, new GeneratorVersionStamp(1));
            _registry = new WorldItemRegistry();
            _collector = new FakeItemCollector();

            _spawner = new WorldItemSpawner(
                _catalog,
                _dropSettings,
                _hudSettings,
                _collector,
                _registry,
                _tracker,
                _grid);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (WorldItemPickup pickup in Object.FindObjectsByType<WorldItemPickup>(FindObjectsSortMode.None))
                Object.DestroyImmediate(pickup.gameObject);

            Object.DestroyImmediate(_catalog);
            Object.DestroyImmediate(_definition);
            Object.DestroyImmediate(_dropSettings);
            Object.DestroyImmediate(_hudSettings);
        }

        [Test]
        public void SpawnFresh_RecordsAddedDeltaInOwningChunkWithLocalPosition()
        {
            // Chunk (2, -1) covers Unity X [200, 300), Z [-50, 0).
            Vector3 position = new Vector3(210f, 1f, -30f);

            _spawner.SpawnFresh(new ItemId(TestItemKey), position, Quaternion.identity, Vector3.zero);

            ChunkDelta pending = _tracker.GetPendingDelta(new ChunkCoordinate(2, -1));
            Assert.AreEqual(1, pending.ItemDeltas.Count);
            WorldItemDelta delta = pending.ItemDeltas[0];
            Assert.AreEqual(DeltaAction.Added, delta.Action);
            Assert.AreEqual(TestItemKey, delta.ItemKey);
            Assert.AreEqual(10f, delta.X);
            Assert.AreEqual(1f, delta.Y);
            Assert.AreEqual(20f, delta.Z);
            Assert.IsTrue(_dirtyRegistry.DirtyChunks.Contains(new ChunkCoordinate(2, -1)));
        }

        [Test]
        public void SpawnFresh_RegistersLiveEntityAndNamesIt()
        {
            WorldItemPickup pickup = _spawner.SpawnFresh(
                new ItemId(TestItemKey),
                new Vector3(210f, 1f, -30f),
                Quaternion.identity,
                Vector3.zero);

            Assert.IsNotNull(pickup);
            Assert.AreEqual("Dropped Persistence Test Item", pickup.gameObject.name);
            Assert.AreEqual(1, _registry.Live.Count);
        }

        [Test]
        public void SpawnPersisted_RestoresEntityWithoutAppendingDelta()
        {
            Vector3 position = new Vector3(205f, 3f, -45f);

            WorldItemPickup pickup = _spawner.SpawnPersisted(99UL, TestItemKey, position);

            Assert.IsNotNull(pickup);
            Assert.AreEqual(position, pickup.transform.position);
            Assert.IsTrue(_registry.IsLive(99UL));
            // The persisted record already lives on disk; a respawn must not
            // create a duplicate Added delta.
            Assert.IsTrue(_tracker.GetPendingDelta(new ChunkCoordinate(2, -1)).IsEmpty);
        }

        [Test]
        public void SpawnPersisted_DuplicateEntityId_ReturnsNullInsteadOfSecondCopy()
        {
            Vector3 position = new Vector3(205f, 3f, -45f);
            _spawner.SpawnPersisted(99UL, TestItemKey, position);

            WorldItemPickup duplicate = _spawner.SpawnPersisted(99UL, TestItemKey, position);

            Assert.IsNull(duplicate);
            Assert.AreEqual(1, _registry.Live.Count);
        }

        [Test]
        public void TryCollect_RecordsRemovedDeltaAndReleasesRegistry()
        {
            var coord = new ChunkCoordinate(2, -1);
            WorldItemPickup pickup = _spawner.SpawnPersisted(77UL, TestItemKey, new Vector3(210f, 1f, -30f));

            Assert.IsTrue(pickup.TryCollect(_collector));

            var items = _tracker.GetPendingDelta(coord).ItemDeltas;
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(77UL, items[0].Id);
            Assert.AreEqual(DeltaAction.Removed, items[0].Action);
            Assert.IsFalse(_registry.IsLive(77UL));
        }

        [Test]
        public void NotifyRemovedFromWorld_Twice_RecordsRemovalOnlyOnce()
        {
            var coord = new ChunkCoordinate(2, -1);
            WorldItemPickup pickup = _spawner.SpawnPersisted(77UL, TestItemKey, new Vector3(210f, 1f, -30f));

            pickup.NotifyRemovedFromWorld();
            pickup.NotifyRemovedFromWorld();

            var items = _tracker.GetPendingDelta(coord).ItemDeltas;
            Assert.AreEqual(1, items.Count);
        }

        [Test]
        public void SpawnFresh_MultipleDrops_AllRegistered()
        {
            for (int i = 0; i < 8; i++)
            {
                WorldItemPickup pickup = _spawner.SpawnFresh(
                    new ItemId(TestItemKey),
                    new Vector3(210f + i, 1f, -30f + i),
                    Quaternion.identity,
                    Vector3.zero);
                Assert.IsNotNull(pickup);
            }

            Assert.AreEqual(8, _registry.Live.Count);
        }

        private static void SetObjectField(ScriptableObject target, string field, object value)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(field);
            Assert.IsNotNull(property, $"Serialized field '{field}' must exist on {target.GetType().Name}.");

            if (value is string stringValue)
                property.stringValue = stringValue;
            else if (value is ItemDefinitionAsset[] array)
            {
                property.arraySize = array.Length;
                for (int i = 0; i < array.Length; i++)
                    property.GetArrayElementAtIndex(i).objectReferenceValue = array[i];
            }
            else
                property.objectReferenceValue = (Object)value;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private sealed class FakeItemCollector : IItemCollector
        {
            public bool TryCollect(ItemId itemId) => true;
            public void RegisterNearbyPickup(IItemPickupTarget pickup) { }
            public void UnregisterNearbyPickup(IItemPickupTarget pickup) { }
            public bool TryCollectNearestPickup() => false;
        }
    }
}
