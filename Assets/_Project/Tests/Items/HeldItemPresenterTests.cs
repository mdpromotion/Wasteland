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
    public class HeldItemPresenterTests
    {
        private HotbarHudSettings _settings;
        private Inventory _inventory;
        private ItemCatalogAsset _catalogAsset;
        private FakeVisualFactory _visualFactory;
        private HeldItemPresenter _presenter;
        private GameObject _cameraObject;
        private readonly List<ItemDefinitionAsset> _createdItemAssets = new();
        private readonly List<Camera> _ambientMainCameras = new();

        [SetUp]
        public void SetUp()
        {
            SuspendAmbientMainCameras();
            _settings = ScriptableObject.CreateInstance<HotbarHudSettings>();
            _catalogAsset = ScriptableObject.CreateInstance<ItemCatalogAsset>();
            SerializeItem(_catalogAsset, "test_item");
            SerializeItem(_catalogAsset, "other_item");

            IItemCatalog domainCatalog = new ItemCatalog(new[]
            {
                CreateDefinition("test_item"),
                CreateDefinition("other_item"),
            });
            _inventory = new Inventory(domainCatalog);
            _visualFactory = new FakeVisualFactory();
        }

        [TearDown]
        public void TearDown()
        {
            if (_presenter != null)
                _presenter.Dispose();

            DestroyImmediateIfAlive(_cameraObject);
            RestoreAmbientMainCameras();

            if (_settings != null)
                Object.DestroyImmediate(_settings);
            if (_catalogAsset != null)
                Object.DestroyImmediate(_catalogAsset);

            foreach (GameObject visual in _visualFactory.Created)
                DestroyImmediateIfAlive(visual);

            foreach (ItemDefinitionAsset itemAsset in _createdItemAssets)
                Object.DestroyImmediate(itemAsset);
            _createdItemAssets.Clear();
        }

        [Test]
        public void Start_AndInventoryChange_WithoutMainCamera_DoesNotThrow()
        {
            CreatePresenter();
            Assert.IsNull(UnityEngine.Camera.main);

            Assert.DoesNotThrow(_presenter.Start);
            Assert.DoesNotThrow(() => _inventory.TryAdd(new ItemId("test_item"), 1));

            Assert.AreEqual(0, _visualFactory.Created.Count,
                "Visuals must not be acquired before the main camera exists.");
        }

        [Test]
        public void ItemAdded_WithMainCamera_CreatesOneEquippedVisualUnderCamera()
        {
            CreateCamera();
            CreatePresenter();
            _presenter.Start();

            _inventory.TryAdd(new ItemId("test_item"), 1);

            Assert.AreEqual(1, _visualFactory.Created.Count);
            GameObject visual = _visualFactory.Created[0];
            Assert.IsTrue(visual.activeSelf);
            // Held view root lives directly under the camera, visual under the view.
            Assert.AreEqual(_cameraObject.transform, visual.transform.parent.parent);
        }

        [Test]
        public void SameItemReAdded_DoesNotRevisualize()
        {
            CreateCamera();
            CreatePresenter();
            _presenter.Start();

            _inventory.TryAdd(new ItemId("test_item"), 1);
            _inventory.TryAdd(new ItemId("test_item"), 1);

            Assert.AreEqual(1, _visualFactory.Created.Count);
        }

        [Test]
        public void ItemRemoved_HidesVisual()
        {
            CreateCamera();
            CreatePresenter();
            _presenter.Start();
            _inventory.TryAdd(new ItemId("test_item"), 1);

            _inventory.TryRemoveSelectedOne(out _);

            Assert.AreEqual(1, _visualFactory.Released.Count);
            Assert.AreEqual("test_item", _visualFactory.Released[0].ToString());
            Assert.IsFalse(_visualFactory.Created[0].activeSelf);
        }

        [Test]
        public void SelectingBetweenSlots_AcquiresVisualPerSelectedItem()
        {
            CreateCamera();
            CreatePresenter();
            _presenter.Start();

            _inventory.TryAdd(new ItemId("test_item"), 1);
            _inventory.TryAdd(new ItemId("other_item"), 1);
            _inventory.SelectSlot(1);

            Assert.AreEqual(2, _visualFactory.Created.Count);
            Assert.AreEqual(new[] { "test_item" },
                _visualFactory.Released.ConvertAll(id => id.ToString()));

            _inventory.SelectSlot(0);

            Assert.AreEqual(3, _visualFactory.Created.Count);
            Assert.AreEqual(new[] { "test_item", "other_item" },
                _visualFactory.Released.ConvertAll(id => id.ToString()));
        }

        [Test]
        public void ItemMissingFromPresentationCatalog_HidesViewWithoutAcquiring()
        {
            CreateCamera();
            CreatePresenter();
            _presenter.Start();
            _inventory.TryAdd(new ItemId("test_item"), 1);

            // Slot holds a definition the presentation catalog does not know.
            _inventory.SetSlotStack(1, CreateDefinition("ghost_item"), 1);
            _inventory.SelectSlot(1);

            Assert.AreEqual(1, _visualFactory.Created.Count, "Unknown items must not acquire a visual.");
            Assert.AreEqual(1, _visualFactory.Released.Count, "The previously shown item is released.");
            Assert.IsFalse(_visualFactory.Created[0].activeSelf);
        }

        [Test]
        public void CameraAppearingAfterStart_PresenterTick_CreatesThePresentation()
        {
            // Regression: at startup the first inventory flow ran before
            // Camera.main was resolvable, leaving the hands empty until the
            // player manually switched slots. The presenter must complete the
            // presentation as soon as the camera shows up.
            CreatePresenter();
            _presenter.Start();
            _inventory.TryAdd(new ItemId("test_item"), 1);

            CreateCamera();
            _presenter.Tick();

            Assert.AreEqual(1, _visualFactory.Created.Count);
            Assert.IsTrue(_visualFactory.Created[0].activeSelf);
            Assert.AreEqual(_cameraObject.transform, _visualFactory.Created[0].transform.parent.parent);
        }

        [Test]
        public void Tick_WhenPresentationSettled_DoesNotReacquire()
        {
            CreateCamera();
            CreatePresenter();
            _presenter.Start();
            _inventory.TryAdd(new ItemId("test_item"), 1);

            _presenter.Tick();
            _presenter.Tick();

            Assert.AreEqual(1, _visualFactory.Created.Count);
        }

        [Test]
        public void InventoryChangeAfterDispose_IsIgnored()
        {
            CreateCamera();
            CreatePresenter();
            _presenter.Start();
            _presenter.Dispose();
            _presenter = null;

            Assert.DoesNotThrow(() => _inventory.TryAdd(new ItemId("test_item"), 1));

            Assert.AreEqual(1, _visualFactory.DisposeCallCount);
            Assert.AreEqual(0, _visualFactory.Created.Count);
        }

        private void CreatePresenter()
        {
            _presenter = new HeldItemPresenter(_inventory, _catalogAsset, _settings, _visualFactory);
        }

        private void CreateCamera()
        {
            _cameraObject = new GameObject("Fake Main Camera") { tag = "MainCamera" };
            _cameraObject.AddComponent<Camera>();
        }

        private void SerializeItem(ItemCatalogAsset catalog, string technicalId)
        {
            ItemDefinitionAsset item = ScriptableObject.CreateInstance<ItemDefinitionAsset>();
            SetField(item, "technicalId", technicalId);
            SetField(item, "displayName", technicalId);

            FieldInfo itemsField = catalog.GetType()
                .GetField("items", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(itemsField, "Item catalog has no 'items' array.");

            ItemDefinitionAsset[] existing = (ItemDefinitionAsset[])itemsField.GetValue(catalog) ?? new ItemDefinitionAsset[0];
            ItemDefinitionAsset[] updated = new ItemDefinitionAsset[existing.Length + 1];
            for (int i = 0; i < existing.Length; i++)
                updated[i] = existing[i];
            updated[existing.Length] = item;
            itemsField.SetValue(catalog, updated);

            _createdItemAssets.Add(item);
        }

        private static void SetField(ScriptableObject target, string fieldName, object value)
        {
            FieldInfo field = target.GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' is not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private static ItemDefinition CreateDefinition(string technicalId)
        {
            return new ItemDefinition(new ItemId(technicalId), technicalId, 1);
        }

        private static void DestroyImmediateIfAlive(GameObject gameObject)
        {
            if (gameObject != null)
                Object.DestroyImmediate(gameObject);
        }

        // EditMode test scenes may already carry a tagged camera (e.g. the
        // editor's open scene). Hiding those cameras makes Camera.main point to
        // the test-created camera only, keeping tests scene-independent.
        private void SuspendAmbientMainCameras()
        {
            _ambientMainCameras.Clear();
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < cameras.Length; i++)
            {
                if (!cameras[i].CompareTag("MainCamera"))
                    continue;

                _ambientMainCameras.Add(cameras[i]);
                cameras[i].enabled = false;
            }
        }

        private void RestoreAmbientMainCameras()
        {
            for (int i = 0; i < _ambientMainCameras.Count; i++)
            {
                if (_ambientMainCameras[i] != null)
                    _ambientMainCameras[i].enabled = true;
            }

            _ambientMainCameras.Clear();
        }

        private sealed class FakeVisualFactory : IHeldItemVisualFactory
        {
            public readonly List<GameObject> Created = new();
            public readonly List<ItemId> Released = new();
            public int DisposeCallCount;

            public GameObject Acquire(ItemDefinitionAsset definition, HeldItemConfig config)
            {
                GameObject visual = new GameObject($"Fake Visual {definition.DisplayName}");
                visual.SetActive(true);
                Created.Add(visual);
                return visual;
            }

            public void Release(ItemId itemId) => Released.Add(itemId);

            public void Dispose() => DisposeCallCount++;
        }
    }
}
