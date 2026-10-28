using System;
using UnityEngine;
using UnityEngine.UI;
using _Project.Features.Items.Infrastructure;

namespace _Project.Features.Items.Presentation
{
    /// <summary>
    /// World-space pickup prompt attached to a dropped item. Renders no UI itself:
    /// it borrows a canvas object from <see cref="PickupBillboardPool"/>, refreshes
    /// it with this item's content and billboards it toward the camera each frame
    /// while visible.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WorldItemPickupBillboard : MonoBehaviour
    {
        private ItemDefinitionAsset _itemDefinition;
        private HotbarHudSettings _settings;
        private PickupBillboardPool _pool;
        private PooledBillboard _billboard;
        private UnityEngine.Camera _camera;
        private bool _visible;

        public void Initialize(
            ItemDefinitionAsset itemDefinition,
            HotbarHudSettings settings,
            PickupBillboardPool pool)
        {
            _itemDefinition = itemDefinition ?? throw new ArgumentNullException(nameof(itemDefinition));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _camera = UnityEngine.Camera.main;
        }

        public void SetVisible(bool visible)
        {
            if (_pool == null)
                return;

            if (visible == _visible)
                return;

            if (visible)
            {
                _visible = true;
                if (_billboard == null)
                    _billboard = _pool.Acquire(transform);

                _billboard.Configure(_itemDefinition, _settings);
                _billboard.Canvas.enabled = true;
            }
            else
            {
                _visible = false;
                if (_billboard != null)
                    _billboard.Canvas.enabled = false;
            }
        }

        public void Release()
        {
            if (_billboard == null || _pool == null)
                return;

            _billboard.Canvas.enabled = false;
            _pool.Release(_billboard);
            _billboard = null;
            _visible = false;
        }

        private void LateUpdate()
        {
            if (_billboard == null || !_billboard.Canvas.enabled)
                return;

            if (!_camera)
                _camera = UnityEngine.Camera.main;
            if (!_camera)
                return;

            if (_billboard.Canvas.worldCamera != _camera)
                _billboard.Canvas.worldCamera = _camera;

            Canvas canvas = _billboard.Canvas;
            canvas.transform.position = transform.position + _settings.PickupBillboardOffset;
            canvas.transform.rotation = Quaternion.LookRotation(
                canvas.transform.position - _camera.transform.position,
                Vector3.up);
        }

        private void OnDestroy()
        {
            Release();
        }
    }
}
