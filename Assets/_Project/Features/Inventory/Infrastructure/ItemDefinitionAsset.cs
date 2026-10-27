using System;
using System.Collections.Generic;
using _Project.Features.Items.Domain;
using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    [CreateAssetMenu(menuName = "Project/Items/Item Definition", fileName = "ItemDefinition")]
    public sealed class ItemDefinitionAsset : ScriptableObject
    {
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        [SerializeField, Min(1)] private int maxStack = 1;
        [SerializeField] private Sprite icon;
        [SerializeField] private Color heldColor = Color.white;
        [SerializeField] private GameObject worldPrefab;
        [SerializeField] private ItemBehaviorAsset[] behaviorAssets = Array.Empty<ItemBehaviorAsset>();

        public ItemId Id => new ItemId(itemId);
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public Color HeldColor => heldColor;
        public GameObject WorldPrefab => worldPrefab;

        public ItemDefinition CreateDomainDefinition()
        {
            ItemBehaviorAsset[] configuredBehaviors = behaviorAssets ?? Array.Empty<ItemBehaviorAsset>();
            var behaviors = new List<ItemBehavior>(configuredBehaviors.Length);
            for (int i = 0; i < configuredBehaviors.Length; i++)
            {
                ItemBehaviorAsset behaviorAsset = configuredBehaviors[i];
                if (behaviorAsset == null)
                    throw new InvalidOperationException($"Item '{name}' has a missing behavior asset at index {i}.");

                behaviors.Add(behaviorAsset.CreateBehavior());
            }

            return new ItemDefinition(Id, displayName, maxStack, behaviors);
        }
    }
}
