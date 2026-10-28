using _Project.Features.Items.Application;
using _Project.Features.Items.Domain;
using _Project.Features.Items.Presentation;
using _Project.Features.ProceduralWorld.Application.Persistence;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Persistence;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Features.Items.Infrastructure
{
    /// <summary>
    /// Creates dropped-item world entities. Used both by the player drop flow
    /// and by the chunk respawn pipeline that restores persisted items.
    /// </summary>
    /// <remarks>
    /// Kept as an interface beside its consumers so the spawn stage (and unit
    /// tests) can depend on the abstraction, not the Unity-heavy implementation.
    /// </remarks>
    public interface IWorldItemSpawner
    {
        /// <summary>Spawns a newly dropped item and records its Added delta.</summary>
        WorldItemPickup SpawnFresh(
            ItemId itemId,
            Vector3 position,
            Quaternion rotation,
            Vector3 velocity);

        /// <summary>
        /// Spawns an item restored from a persisted delta. Does not record a new
        /// delta — the persisted record is already on disk. Returns null when the
        /// entity id is already alive (duplicate protection).
        /// </summary>
        WorldItemPickup SpawnPersisted(ulong entityId, string itemKey, Vector3 position);
    }

    public sealed class WorldItemSpawner : IWorldItemSpawner
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private readonly ItemCatalogAsset _catalog;
        private readonly ItemDropSettings _settings;
        private readonly HotbarHudSettings _hudSettings;
        private readonly IItemCollector _itemCollector;
        private readonly WorldItemRegistry _registry;
        private readonly IChunkMutationTracker _mutationTracker;
        private readonly ChunkGrid _chunkGrid;

        private PickupBillboardPool _billboardPool;

        public WorldItemSpawner(
            ItemCatalogAsset catalog,
            ItemDropSettings settings,
            HotbarHudSettings hudSettings,
            IItemCollector itemCollector,
            WorldItemRegistry registry,
            IChunkMutationTracker mutationTracker,
            ChunkGrid chunkGrid)
        {
            _catalog = catalog ?? throw new System.ArgumentNullException(nameof(catalog));
            _settings = settings ?? throw new System.ArgumentNullException(nameof(settings));
            _hudSettings = hudSettings ?? throw new System.ArgumentNullException(nameof(hudSettings));
            _itemCollector = itemCollector ?? throw new System.ArgumentNullException(nameof(itemCollector));
            _registry = registry ?? throw new System.ArgumentNullException(nameof(registry));
            _mutationTracker = mutationTracker ?? throw new System.ArgumentNullException(nameof(mutationTracker));
            _chunkGrid = chunkGrid ?? throw new System.ArgumentNullException(nameof(chunkGrid));
        }

        public WorldItemPickup SpawnFresh(
            ItemId itemId,
            Vector3 position,
            Quaternion rotation,
            Vector3 velocity)
        {
            return Spawn(
                itemId.Value,
                NewEntityId(),
                position,
                rotation,
                velocity,
                recordAdded: true);
        }

        public WorldItemPickup SpawnPersisted(ulong entityId, string itemKey, Vector3 position)
        {
            return Spawn(
                itemKey,
                entityId,
                position,
                Quaternion.identity,
                Vector3.zero,
                recordAdded: false);
        }

        private WorldItemPickup Spawn(
            string itemKey,
            ulong entityId,
            Vector3 position,
            Quaternion rotation,
            Vector3 velocity,
            bool recordAdded)
        {
            if (_registry.IsLive(entityId))
                return null;

            ItemId itemId = new ItemId(itemKey);
            ItemDefinitionAsset definition = _catalog.Get(itemId);
            ChunkCoordinate chunk = _chunkGrid.ToChunkCoordinate(position);

            GameObject body = definition.WorldPrefab != null
                ? Object.Instantiate(definition.WorldPrefab, position, rotation)
                : CreateFallbackItem(definition, position);

            body.name = $"Dropped {definition.DisplayName}";
            ConfigureCollider(body, _settings.PickupRange);

            Rigidbody rigidbody = body.GetComponent<Rigidbody>();
            if (rigidbody == null)
                rigidbody = body.AddComponent<Rigidbody>();

            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;
            rigidbody.linearVelocity = velocity;

            WorldItemPickup pickup = body.GetComponent<WorldItemPickup>();
            if (pickup == null)
                pickup = body.AddComponent<WorldItemPickup>();

            Transform collectorAnchor = _itemCollector is Component collectorComponent
                ? collectorComponent.transform
                : null;

            var handle = new DroppedItemHandle(_mutationTracker, _registry, chunk, entityId);
            pickup.Initialize(
                itemId,
                definition,
                _hudSettings,
                EnsureBillboardPool(),
                _itemCollector,
                collectorAnchor,
                handle);

            if (recordAdded)
            {
                Vector3 localPosition = TransformPositionToChunkLocal(position, chunk);
                _mutationTracker.RecordItemAdded(
                    chunk,
                    new WorldItemDelta(entityId, DeltaAction.Added, itemId.Value,
                        localPosition.x, localPosition.y, localPosition.z));
            }

            _registry.Register(entityId, pickup);
            return pickup;
        }

        private Vector3 TransformPositionToChunkLocal(Vector3 position, ChunkCoordinate chunk)
        {
            Vector2 chunkOrigin = _chunkGrid.ToWorldOffset(chunk);
            return new Vector3(
                position.x - chunkOrigin.x,
                position.y,
                position.z - chunkOrigin.y);
        }

        private PickupBillboardPool EnsureBillboardPool() =>
            _billboardPool ??= new PickupBillboardPool(_hudSettings);

        private static ulong NewEntityId()
        {
            var guidBytes = Guid.NewGuid().ToByteArray();
            return BitConverter.ToUInt64(guidBytes, 0);
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

        private static void ConfigureCollider(GameObject item, float pickupRange)
        {
            Collider[] colliders = item.GetComponentsInChildren<Collider>();
            bool hasSolidCollider = false;
            SphereCollider pickupTrigger = null;
            for (int i = 0; i < colliders.Length; i++)
            {
                hasSolidCollider |= !colliders[i].isTrigger;
                if (pickupTrigger == null && colliders[i].isTrigger && colliders[i] is SphereCollider sphere)
                    pickupTrigger = sphere;
            }

            if (!hasSolidCollider)
                item.AddComponent<BoxCollider>();

            if (pickupTrigger == null)
            {
                pickupTrigger = item.AddComponent<SphereCollider>();
                pickupTrigger.isTrigger = true;
            }

            pickupTrigger.radius = pickupRange;
        }
    }
}
