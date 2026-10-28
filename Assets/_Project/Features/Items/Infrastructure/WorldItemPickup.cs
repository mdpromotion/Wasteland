using System.Collections.Generic;
using _Project.Features.Items.Application;
using _Project.Features.Items.Domain;
using _Project.Features.Items.Presentation;
using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class WorldItemPickup : MonoBehaviour, IItemPickupTarget
    {
        private readonly HashSet<Collider> _playerColliders = new HashSet<Collider>();
        private ItemId _itemId;
        private ItemDefinitionAsset _definition;
        private HotbarHudSettings _hudSettings;
        private PickupBillboardPool _billboardPool;
        private IItemCollector _collector;
        private Transform _collectorAnchor;
        private WorldItemPickupBillboard _billboard;
        private bool _initialized;

        public float Distance { get; private set; }

        public void Initialize(
            ItemId itemId,
            ItemDefinitionAsset definition,
            HotbarHudSettings hudSettings,
            PickupBillboardPool billboardPool,
            IItemCollector collector,
            Transform collectorAnchor)
        {
            _itemId = itemId;
            _definition = definition;
            _hudSettings = hudSettings;
            _billboardPool = billboardPool;
            _collector = collector;
            _collectorAnchor = collectorAnchor;
            _initialized = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_initialized)
                return;

            if (!BelongsToCollector(other))
                return;

            if (!_playerColliders.Add(other))
                return;

            UpdateDistance(other);
            if (_playerColliders.Count == 1 && _collector != null)
                _collector.RegisterNearbyPickup(this);
        }

        private void OnTriggerStay(Collider other)
        {
            if (_playerColliders.Contains(other))
                UpdateDistance(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_playerColliders.Remove(other))
                return;

            if (_playerColliders.Count > 0)
                return;

            // The collector reference is kept: the player can re-enter the zone,
            // and re-registering on the next OnTriggerEnter requires it.
            _collector?.UnregisterNearbyPickup(this);
            SetPromptVisible(false);
        }

        public bool TryCollect(IItemCollector collector)
        {
            if (!_initialized || collector == null || collector != _collector)
                return false;
            if (!collector.TryCollect(_itemId))
                return false;

            Destroy(gameObject);
            return true;
        }

        /// <summary>
        /// Billboard shown only on demand (see PickupPromptPresenter) and created
        /// lazily, so distant or capped pickups never build UI at all.
        /// </summary>
        public void SetPromptVisible(bool visible)
        {
            if (!visible)
            {
                if (_billboard != null)
                    _billboard.SetVisible(false);
                return;
            }

            if (_billboard == null)
            {
                _billboard = gameObject.AddComponent<WorldItemPickupBillboard>();
                _billboard.Initialize(_definition, _hudSettings, _billboardPool);
            }

            _billboard.SetVisible(true);
        }

        private void UpdateDistance(Collider other)
        {
            Vector3 collectorPosition = other.attachedRigidbody != null
                ? other.attachedRigidbody.position
                : other.bounds.center;
            Distance = Vector3.Distance(transform.position, collectorPosition);
        }

        private bool BelongsToCollector(Collider other)
        {
            if (_collectorAnchor != null)
                return other.transform.IsChildOf(_collectorAnchor);

            // Fallback for pickups placed in the scene without a wired collector:
            // discover the collector through its components (allocating) once.
            IItemCollector discovered = FindCollector(other);
            if (discovered == null)
                return false;

            _collector = discovered;
            if (discovered is Component component)
                _collectorAnchor = component.transform;

            return true;
        }

        private static IItemCollector FindCollector(Collider other)
        {
            MonoBehaviour[] candidates = other.GetComponentsInParent<MonoBehaviour>();
            for (int i = 0; i < candidates.Length; i++)
                if (candidates[i] is IItemCollector collector)
                    return collector;

            return null;
        }

        private void OnDestroy()
        {
            if (_collector != null)
                _collector.UnregisterNearbyPickup(this);

            if (_billboard != null)
                _billboard.Release();
        }
    }
}
