using _Project.Features.Items.Application;
using _Project.Features.Items.Domain;
using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class WorldItemPickup : MonoBehaviour
    {
        private ItemId _itemId;
        private bool _initialized;

        public void Initialize(ItemId itemId)
        {
            _itemId = itemId;
            _initialized = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_initialized)
                return;

            MonoBehaviour[] candidates = other.GetComponentsInParent<MonoBehaviour>();
            for (int i = 0; i < candidates.Length; i++)
            {
                if (!(candidates[i] is IItemCollector collector) || !collector.TryCollect(_itemId))
                    continue;

                Destroy(gameObject);
                return;
            }
        }
    }
}
