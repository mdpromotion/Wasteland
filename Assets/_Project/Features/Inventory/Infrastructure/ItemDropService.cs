using _Project.Features.Items.Application;
using _Project.Features.Items.Domain;
using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    public sealed class ItemDropService : IItemDropService
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private readonly Inventory _inventory;
        private readonly ItemCatalogAsset _catalog;
        private readonly ItemDropSettings _settings;
        private UnityEngine.Camera _camera;

        public ItemDropService(
            Inventory inventory,
            ItemCatalogAsset catalog,
            ItemDropSettings settings)
        {
            _inventory = inventory;
            _catalog = catalog;
            _settings = settings;
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

            ItemDefinitionAsset definition = _catalog.Get(stack.Item.Id);
            Vector3 spawnPosition = _camera.transform.TransformPoint(_settings.SpawnLocalOffset);
            GameObject droppedObject = definition.WorldPrefab != null
                ? Object.Instantiate(definition.WorldPrefab, spawnPosition, _camera.transform.rotation)
                : CreateFallbackItem(definition, spawnPosition);

            droppedObject.name = $"Dropped {definition.DisplayName}";
            ConfigureCollider(droppedObject);
            Rigidbody body = droppedObject.GetComponent<Rigidbody>();
            if (body == null)
                body = droppedObject.AddComponent<Rigidbody>();

            body.isKinematic = false;
            body.useGravity = true;
            body.linearVelocity = _camera.transform.forward * _settings.ForwardSpeed +
                                 Vector3.up * _settings.UpwardSpeed;

            WorldItemPickup pickup = droppedObject.GetComponent<WorldItemPickup>();
            if (pickup == null)
                pickup = droppedObject.AddComponent<WorldItemPickup>();
            pickup.Initialize(stack.Item.Id);

            if (!_inventory.TryRemoveSelectedOne(out _))
            {
                Object.Destroy(droppedObject);
                return false;
            }

            if (_settings.WorldObjectLifetime > 0f)
                Object.Destroy(droppedObject, _settings.WorldObjectLifetime);

            return true;
        }

        private static GameObject CreateFallbackItem(ItemDefinitionAsset definition, Vector3 position)
        {
            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.transform.position = position;

            Renderer renderer = item.GetComponent<Renderer>();
            if (renderer != null)
            {
                var propertyBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, definition.HeldColor);
                propertyBlock.SetColor(ColorId, definition.HeldColor);
                renderer.SetPropertyBlock(propertyBlock);
            }

            return item;
        }

        private static void ConfigureCollider(GameObject item)
        {
            Collider[] colliders = item.GetComponentsInChildren<Collider>();
            bool hasSolidCollider = false;
            bool hasTriggerCollider = false;
            for (int i = 0; i < colliders.Length; i++)
            {
                hasSolidCollider |= !colliders[i].isTrigger;
                hasTriggerCollider |= colliders[i].isTrigger;
            }

            if (!hasSolidCollider)
                item.AddComponent<BoxCollider>();

            if (!hasTriggerCollider)
            {
                SphereCollider pickupTrigger = item.AddComponent<SphereCollider>();
                pickupTrigger.radius = 0.45f;
                pickupTrigger.isTrigger = true;
            }

        }
    }
}
