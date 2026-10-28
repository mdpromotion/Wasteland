using _Project.Features.Items.Application;
using _Project.Features.Items.Domain;
using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    public sealed class ItemDropService : IItemDropService
    {
        private readonly Inventory _inventory;
        private readonly ItemDropSettings _settings;
        private readonly IWorldItemSpawner _spawner;

        private UnityEngine.Camera _camera;

        public ItemDropService(
            Inventory inventory,
            ItemDropSettings settings,
            IWorldItemSpawner spawner)
        {
            _inventory = inventory;
            _settings = settings;
            _spawner = spawner ?? throw new System.ArgumentNullException(nameof(spawner));
        }

        public bool TryDropSelected()
        {
            InventoryStack stack = _inventory.GetSlot(_inventory.SelectedSlot);
            if (stack == null)
                return false;

            if (!_camera)
                _camera = UnityEngine.Camera.main;
            if (!_camera)
                return false;

            Vector3 spawnPosition = _camera.transform.TransformPoint(_settings.SpawnLocalOffset);
            Vector3 velocity = _camera.transform.forward * _settings.ForwardSpeed +
                               Vector3.up * _settings.UpwardSpeed;

            WorldItemPickup spawned = _spawner.SpawnFresh(
                stack.Item.Id,
                spawnPosition,
                _camera.transform.rotation,
                velocity);

            if (spawned == null)
                return false;

            if (!_inventory.TryRemoveSelectedOne(out _))
            {
                // Roll back: the entity never actually left the inventory.
                spawned.NotifyRemovedFromWorld();
                Object.Destroy(spawned.gameObject);
                return false;
            }

            return true;
        }
    }
}
