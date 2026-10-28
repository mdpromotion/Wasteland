using System;
using System.Collections.Generic;
using _Project.Features.Items.Domain;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Features.Items.Infrastructure
{
    [CreateAssetMenu(menuName = "Project/Items/Item Definition", fileName = "ItemDefinition")]
    public class ItemDefinitionAsset : ScriptableObject
    {
        [SerializeField, FormerlySerializedAs("itemId"), Tooltip("Stable technical ID used by saves and code.")] private string technicalId;
        [SerializeField] private string displayName;
        [SerializeField] private ItemCategoryAsset category;
        [SerializeField, Min(1)] private int maxStack = 1;
        [SerializeField] private Sprite icon;
        [SerializeField] private Color heldColor = Color.white;
        [SerializeField] private GameObject worldPrefab;
        [SerializeField] private ItemBehaviorAsset[] behaviorAssets = Array.Empty<ItemBehaviorAsset>();

        public ItemId Id => new ItemId(technicalId);
        public string TechnicalId => technicalId;
        public string DisplayName => displayName;
        public ItemCategoryAsset Category => category;
        public int MaxStack => maxStack;
        public Sprite Icon => icon;
        public Color HeldColor => heldColor;
        public GameObject WorldPrefab => worldPrefab;

        public virtual ItemDefinition CreateDomainDefinition()
        {
            return new ItemDefinition(Id, displayName, maxStack,
                category == null ? string.Empty : category.TechnicalId,
                category == null ? string.Empty : category.DisplayName,
                CreateBehaviors());
        }

        protected List<ItemBehavior> CreateBehaviors()
        {
            ItemBehaviorAsset[] configured = behaviorAssets ?? Array.Empty<ItemBehaviorAsset>();
            var behaviors = new List<ItemBehavior>(configured.Length);
            for (int i = 0; i < configured.Length; i++)
            {
                if (configured[i] == null)
                    throw new InvalidOperationException($"Item '{name}' has a missing behavior asset at index {i}.");
                behaviors.Add(configured[i].CreateBehavior());
            }
            return behaviors;
        }
    }
}