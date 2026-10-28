using _Project.Features.Items.Domain;
using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    [CreateAssetMenu(menuName = "Project/Items/Behaviors/Consumable", fileName = "ConsumableBehavior")]
    public sealed class ConsumableItemBehaviorAsset : ItemBehaviorAsset
    {
        public override ItemBehavior CreateBehavior() => new ConsumableItemBehavior();
    }
}
