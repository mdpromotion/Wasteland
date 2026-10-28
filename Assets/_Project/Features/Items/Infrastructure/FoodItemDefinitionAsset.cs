using _Project.Features.Items.Domain;
using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    [CreateAssetMenu(menuName = "Project/Items/Food", fileName = "FoodItem")]
    public sealed class FoodItemDefinitionAsset : ItemDefinitionAsset
    {
        [SerializeField, Min(0f)] private float saturation = 1f;

        public override ItemDefinition CreateDomainDefinition()
        {
            return new FoodItemDefinition(Id, DisplayName, MaxStack,
                Category == null ? string.Empty : Category.TechnicalId,
                Category == null ? string.Empty : Category.DisplayName,
                saturation, CreateBehaviors());
        }
    }
}