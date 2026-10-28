using System;
using _Project.Features.Items.Domain;
using _Project.Features.Items.Infrastructure;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Features.Items.Presentation
{
    public sealed class HeldItemPresenter : IStartable, ITickable, IDisposable
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private readonly Inventory _inventory;
        private readonly ItemCatalogAsset _catalog;
        private readonly HotbarHudSettings _settings;
        private Transform _itemTransform;
        private Renderer _itemRenderer;
        private MaterialPropertyBlock _propertyBlock;
        private ItemId? _displayedItemId;
        private float _equipProgress;
        private bool _isEquipping;

        public HeldItemPresenter(
            Inventory inventory,
            ItemCatalogAsset catalog,
            HotbarHudSettings settings)
        {
            _inventory = inventory;
            _catalog = catalog;
            _settings = settings;
        }

        public void Start()
        {
            UnityEngine.Camera camera = UnityEngine.Camera.main;
            if (camera == null)
                return;

            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name = "Held Item View";
            UnityEngine.Object.Destroy(item.GetComponent<Collider>());
            _itemTransform = item.transform;
            _itemTransform.SetParent(camera.transform, false);
            _itemRenderer = item.GetComponent<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
            _inventory.Changed += OnInventoryChanged;
            OnInventoryChanged();
        }

        private void OnInventoryChanged()
        {
            if (_itemTransform == null || _itemRenderer == null)
                return;

            ItemDefinition selectedItem = _inventory.SelectedItem;
            if (selectedItem == null)
            {
                _displayedItemId = null;
                _itemRenderer.enabled = false;
                _isEquipping = false;
                return;
            }

            if (_displayedItemId.HasValue && _displayedItemId.Value == selectedItem.Id)
                return;

            _displayedItemId = selectedItem.Id;
            _itemRenderer.enabled = true;
            _itemTransform.localScale = _settings.HeldItemScale;

            Color color = _catalog.Get(selectedItem.Id).HeldColor;
            _itemRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BaseColorId, color);
            _propertyBlock.SetColor(ColorId, color);
            _itemRenderer.SetPropertyBlock(_propertyBlock);

            _equipProgress = 0f;
            _isEquipping = true;
            _itemTransform.localPosition = _settings.StowedItemPosition;
        }

        public void Tick()
        {
            if (!_isEquipping || _itemTransform == null)
                return;

            _equipProgress = Mathf.Min(1f, _equipProgress + Time.deltaTime / _settings.EquipDuration);
            float easedProgress = _equipProgress * _equipProgress * (3f - 2f * _equipProgress);

            Quaternion heldRotation = Quaternion.Euler(_settings.HeldItemRotation);
            Quaternion stowedRotation = Quaternion.Euler(_settings.HeldItemRotation + new Vector3(70f, 0f, 0f));
            _itemTransform.localPosition = Vector3.Lerp(
                _settings.StowedItemPosition,
                _settings.HeldItemPosition,
                easedProgress);
            _itemTransform.localRotation = Quaternion.Slerp(stowedRotation, heldRotation, easedProgress);

            if (_equipProgress >= 1f)
                _isEquipping = false;
        }

        public void Dispose()
        {
            _inventory.Changed -= OnInventoryChanged;
            if (_itemTransform != null)
                UnityEngine.Object.Destroy(_itemTransform.gameObject);
        }
    }
}
