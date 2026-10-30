using _Project.Features.Items.Presentation;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Tests.Items
{
    public class HeldItemViewTests
    {
        private HotbarHudSettings _settings;
        private HeldItemConfig _config;
        private HeldItemView _view;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<HotbarHudSettings>();
            _config = ScriptableObject.CreateInstance<HeldItemConfig>();
            _view = new GameObject("Held Item View").AddComponent<HeldItemView>();
            _view.Construct(_settings);
        }

        [TearDown]
        public void TearDown()
        {
            if (_view != null)
                Object.DestroyImmediate(_view.gameObject);
            if (_settings != null)
                Object.DestroyImmediate(_settings);
            if (_config != null)
                Object.DestroyImmediate(_config);
        }

        [Test]
        public void Construct_NullSettings_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(
                () => new GameObject("Broken View").AddComponent<HeldItemView>().Construct(null));
        }

        [Test]
        public void Show_AttachesVisualUnderViewAndKeepsItActive()
        {
            GameObject visual = new GameObject("Visual");

            _view.Show(visual, null);

            Assert.AreEqual(_view.transform, visual.transform.parent);
            Assert.IsTrue(visual.activeSelf);
            Assert.IsTrue(_view.enabled, "The equip motion must start on show.");
        }

        [Test]
        public void Show_PlacesVisualAtStowedPoseWithHeldScale()
        {
            GameObject visual = new GameObject("Visual");

            _view.Show(visual, null);

            Assert.AreEqual(_settings.StowedItemPosition, visual.transform.localPosition);
            Assert.AreEqual(_settings.HeldItemScale, visual.transform.localScale);
        }

        [Test]
        public void Show_NullAfterShown_DetachesAndDeactivatesPreviousVisual()
        {
            GameObject visual = new GameObject("Visual");
            _view.Show(visual, null);

            _view.Show(null, null);

            Assert.IsNull(visual.transform.parent);
            Assert.IsFalse(visual.activeSelf);
            Assert.IsNull(_view.Visual);
            Assert.IsFalse(_view.enabled);
        }

        [Test]
        public void Show_WithConfig_AppliesConfiguredHeldScale()
        {
            SetField(_config, "heldPosition", new Vector3(0.1f, 0.2f, 0.3f));
            SetField(_config, "heldRotation", new Vector3(5f, 10f, 15f));
            SetField(_config, "heldScale", new Vector3(0.4f, 0.4f, 0.4f));
            GameObject visual = new GameObject("Visual");

            _view.Show(visual, _config);

            Assert.AreEqual(_config.HeldScale, visual.transform.localScale);
        }

        [Test]
        public void EquipMotion_CompletesAtConfiguredHeldPose()
        {
            SetField(_config, "heldPosition", new Vector3(0.1f, 0.55f, 0.3f));
            SetField(_config, "heldRotation", new Vector3(5f, 10f, 15f));
            GameObject visual = new GameObject("Visual");
            _view.Show(visual, _config);

            // Force the animation to its end deterministically (EditMode deltas
            // are not guaranteed) and run one motion step.
            SetField(_view, "_equipProgress", 1f);
            InvokePrivateUpdate(_view);

            Assert.AreEqual(_config.HeldPosition, visual.transform.localPosition);
            Assert.IsTrue(Quaternion.Angle(
                    Quaternion.Euler(_config.HeldRotation), visual.transform.localRotation) < 0.01f,
                $"Expected held rotation {Quaternion.Euler(_config.HeldRotation)}, got {visual.transform.localRotation}.");
        }

        private static void InvokePrivateUpdate(HeldItemView view)
        {
            System.Reflection.MethodInfo update = typeof(HeldItemView)
                .GetMethod("Update", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(update, "HeldItemView has no private Update.");
            update.Invoke(view, null);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            System.Reflection.FieldInfo field = target.GetType()
                .GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' is not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        [Test]
        public void Hide_DetachesVisual()
        {
            GameObject visual = new GameObject("Visual");
            _view.Show(visual, null);

            _view.Hide();

            Assert.IsNull(visual.transform.parent);
            Assert.IsFalse(visual.activeSelf);
            Assert.IsFalse(_view.enabled);
        }

        [Test]
        public void Show_CalledTwiceWithSameVisual_RestartsFromStowedPose()
        {
            GameObject visual = new GameObject("Visual");
            _view.Show(visual, null);
            _view.Hide();
            _view.Show(visual, null);

            Assert.AreEqual(_settings.StowedItemPosition, visual.transform.localPosition);
            Assert.AreEqual(_view.transform, visual.transform.parent);
            Assert.IsTrue(_view.enabled);
        }
    }
}
