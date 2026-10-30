using System;
using System.Collections.Generic;
using _Project.Features.Items.Domain;
using _Project.Features.Items.Infrastructure;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Features.Items.Presentation
{
    /// <summary>
    /// Builds the view-model GameObject shown in the player's hands for an
    /// equipped item. Visuals are created once per item and cached: switching
    /// hotbar slots back and forth only toggles activity, never re-instantiates.
    /// </summary>
    /// <remarks>
    /// Visual source priority: <see cref="HeldItemConfig.VisualPrefab"/>
    /// (dedicated first-person model), then <see cref="ItemDefinitionAsset.WorldPrefab"/>
    /// (re-used pickup visual), then a colored cube fallback shared with the
    /// dropped-item spawner.
    /// </remarks>
    public sealed class HeldItemVisualFactory : IHeldItemVisualFactory
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private readonly Dictionary<ItemId, GameObject> _cache = new();

        public GameObject Acquire(ItemDefinitionAsset definition, HeldItemConfig config)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            ItemId itemId = definition.Id;
            if (_cache.TryGetValue(itemId, out GameObject cached) && cached != null)
            {
                cached.SetActive(true);
                return cached;
            }

            GameObject visual = CreateVisual(definition, config);
            visual.name = $"Held {definition.DisplayName}";
            _cache[itemId] = visual;
            return visual;
        }

        public void Release(ItemId itemId)
        {
            if (_cache.TryGetValue(itemId, out GameObject cached) && cached != null)
                cached.SetActive(false);
        }

        public void Dispose()
        {
            foreach (KeyValuePair<ItemId, GameObject> entry in _cache)
            {
                if (entry.Value != null)
                    Object.DestroyImmediate(entry.Value);
            }

            _cache.Clear();
        }

        private GameObject CreateVisual(ItemDefinitionAsset definition, HeldItemConfig config)
        {
            GameObject source = config != null && config.VisualPrefab != null
                ? config.VisualPrefab
                : definition.WorldPrefab;
            return source != null ? PrepareWorldVisual(source) : CreateFallbackCube(definition);
        }

        private GameObject PrepareWorldVisual(GameObject source)
        {
            GameObject visual = Object.Instantiate(source);

            // The held copy is never a physical entity: no colliders to hit the
            // raycaster, no rigidbody to fall, no pickup logic re-registered.
            StripWorldEntityBehaviour(visual);

            // Drop the editor placement pose so the visual pivots exactly where
            // the held-item view anchors it, matching the world spawner behaviour.
            Transform root = visual.transform;
            root.localPosition = Vector3.zero;
            root.localRotation = Quaternion.identity;
            return visual;
        }

        private static void StripWorldEntityBehaviour(GameObject visual)
        {
            // DestroyImmediate keeps the strip deterministic (across Play and
            // EditMode) because the held copy must never simulate a physics step
            // nor re-run pickup registration. The path executes once per item.
            Rigidbody[] rigidbodies = visual.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < rigidbodies.Length; i++)
                DestroyComponent(rigidbodies[i]);

            Collider[] colliders = visual.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
                DestroyComponent(colliders[i]);

            WorldItemPickup[] pickups = visual.GetComponentsInChildren<WorldItemPickup>(true);
            for (int i = 0; i < pickups.Length; i++)
                DestroyComponent(pickups[i]);
        }

        /// <summary>Immediate in edit mode like <see cref="WorldItemPickup.DestroyEntity"/> does.</summary>
        private static void DestroyComponent(Component component)
        {
            if (component == null)
                return;

            if (UnityEngine.Application.isPlaying)
                Object.Destroy(component);
            else
                Object.DestroyImmediate(component);
        }

        private GameObject CreateFallbackCube(ItemDefinitionAsset definition)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyComponent(cube.GetComponent<Collider>());

            Renderer renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                var propertyBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, definition.HeldColor);
                propertyBlock.SetColor(ColorId, definition.HeldColor);
                renderer.SetPropertyBlock(propertyBlock);
            }

            return cube;
        }
    }
}
