using System;
using _Project.Features.Items.Domain;
using _Project.Features.Items.Infrastructure;
using UnityEngine;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace _Project.Features.Items.Presentation
{
    /// <summary>
    /// Maps the player's inventory selection onto the held-item view: whenever
    /// the selected item changes, the presentation layer shows that item's
    /// visual in the player's hands, hides it for an empty slot, and releases
    /// the previous visual back to its factory.
    /// </summary>
    /// <remarks>
    /// The presenter owns no transforms and no animation state — all Unity work
    /// is delegated to <see cref="HeldItemView"/> and <see cref="IHeldItemVisualFactory"/>;
    /// this keeps the mapping logic thin and replaceable (e.g. hand-bone anchoring
    /// can later re-route visuals without touching inventory reactivity).
    /// </remarks>
    public sealed class HeldItemPresenter : IStartable, ITickable, IDisposable
    {
        private readonly Inventory _inventory;
        private readonly ItemCatalogAsset _catalog;
        private readonly HotbarHudSettings _settings;
        private readonly IHeldItemVisualFactory _visualFactory;

        private HeldItemView _view;
        /// <summary>Item the inventory currently asks to present (may be null).</summary>
        private ItemId? _desiredItemId;
        /// <summary>Item the active view actually presents.</summary>
        private ItemId? _shownItemId;
        /// <summary>True while the presented state lags behind the desired one.</summary>
        private bool _presentationDirty;

        public HeldItemPresenter(
            Inventory inventory,
            ItemCatalogAsset catalog,
            HotbarHudSettings settings,
            IHeldItemVisualFactory visualFactory)
        {
            _inventory = inventory;
            _catalog = catalog;
            _settings = settings;
            _visualFactory = visualFactory;
        }

        public void Start()
        {
            _inventory.Changed += OnInventoryChanged;
            OnInventoryChanged();
        }

        public void Tick()
        {
            // No view present / camera not yet available: retry the delayed
            // presentation every frame until it succeeds. When everything is
            // settled this costs a single comparison per frame.
            if (_presentationDirty || (_view == null && _desiredItemId != null))
                RefreshPresentation();
        }

        public void Dispose()
        {
            _inventory.Changed -= OnInventoryChanged;
            _visualFactory.Dispose();

            if (_view != null && UnityEngine.Application.isPlaying)
                Object.Destroy(_view.gameObject);
            else if (_view != null)
                Object.DestroyImmediate(_view.gameObject);
        }

        private void OnInventoryChanged()
        {
            _desiredItemId = ResolveSelectedItemId();
            _presentationDirty = true;
            RefreshPresentation();
        }

        private void RefreshPresentation()
        {
            if (_desiredItemId == _shownItemId && _view != null)
            {
                _presentationDirty = false;
                return;
            }

            ReleaseShownVisual();
            if (_desiredItemId == null)
            {
                _presentationDirty = false;
                return;
            }

            if (!EnsureView())
                return;

            if (!_catalog.TryGet(_desiredItemId.Value, out ItemDefinitionAsset definition))
            {
                _presentationDirty = false;
                return;
            }

            GameObject visual = _visualFactory.Acquire(definition, definition.HeldItemConfig);
            _view.Show(visual, definition.HeldItemConfig);
            _shownItemId = _desiredItemId;
            _presentationDirty = false;
        }

        private ItemId? ResolveSelectedItemId()
        {
            return _inventory.SelectedItem?.Id;
        }

        private void ReleaseShownVisual()
        {
            if (_shownItemId.HasValue)
            {
                _visualFactory.Release(_shownItemId.Value);
                _shownItemId = null;
            }

            if (_view != null)
                _view.Hide();
        }

        private bool EnsureView()
        {
            if (_view != null)
                return true;

            UnityEngine.Camera camera = UnityEngine.Camera.main;
            if (camera == null)
                return false;

            GameObject viewObject = new GameObject("Held Item View");
            viewObject.transform.SetParent(camera.transform, false);
            viewObject.transform.localPosition = Vector3.zero;
            viewObject.transform.localRotation = Quaternion.identity;

            _view = viewObject.AddComponent<HeldItemView>();
            _view.Construct(_settings);
            return true;
        }
    }
}
