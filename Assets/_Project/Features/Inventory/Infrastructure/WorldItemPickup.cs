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
        private IItemCollector _collector;
        private WorldItemPickupBillboard _billboard;
        private bool _initialized;

        public float Distance { get; private set; }

        public void Initialize(ItemId itemId, WorldItemPickupBillboard billboard)
        {
            _itemId = itemId;
            _billboard = billboard;
            _initialized = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_initialized)
                return;

            IItemCollector collector = FindCollector(other);
            if (collector == null || (_collector != null && _collector != collector))
                return;

            _collector = collector;
            if (!_playerColliders.Add(other))
                return;

            UpdateDistance(other);
            if (_playerColliders.Count == 1)
            {
                _collector.RegisterNearbyPickup(this);
                _billboard.SetVisible(true);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (_playerColliders.Contains(other))
                UpdateDistance(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (_collector == null || !_playerColliders.Remove(other))
                return;

            if (_playerColliders.Count > 0)
                return;

            _collector.UnregisterNearbyPickup(this);
            _collector = null;
            _billboard.SetVisible(false);
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

        private void UpdateDistance(Collider other)
        {
            Vector3 collectorPosition = other.attachedRigidbody != null
                ? other.attachedRigidbody.position
                : other.bounds.center;
            Distance = Vector3.Distance(transform.position, collectorPosition);
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
        }
    }
}
