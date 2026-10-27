using _Project.Features.Items.Domain;
using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    [CreateAssetMenu(menuName = "Project/Items/Tool", fileName = "ToolItem")]
    public sealed class ToolItemDefinitionAsset : ItemDefinitionAsset
    {
        [SerializeField, Min(0f)] private float durability = 100f;
        [SerializeField, Min(0f)] private float miningSpeed = 1f;
        [SerializeField, Min(0f)] private float miningPower = 1f;

        public override ItemDefinition CreateDomainDefinition()
        {
            var behaviors = CreateBehaviors();
            for (int i = 0; i < behaviors.Count; i++)
                if (behaviors[i] is TreeChoppingItemBehavior)
                    behaviors[i] = new TreeChoppingItemBehavior(miningPower);

            return new ToolItemDefinition(Id, DisplayName, MaxStack,
                Category == null ? string.Empty : Category.TechnicalId,
                Category == null ? string.Empty : Category.DisplayName,
                durability, miningSpeed, miningPower, behaviors);
        }
    }
}