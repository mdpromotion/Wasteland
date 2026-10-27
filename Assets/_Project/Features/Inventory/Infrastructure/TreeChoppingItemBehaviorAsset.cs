using _Project.Features.Items.Domain;
using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    [CreateAssetMenu(menuName = "Project/Items/Behaviors/Tree Chopping", fileName = "TreeChoppingBehavior")]
    public sealed class TreeChoppingItemBehaviorAsset : ItemBehaviorAsset
    {
        [SerializeField, Min(0.01f)] private float damage = 1f;

        public override ItemBehavior CreateBehavior() => new TreeChoppingItemBehavior(damage);
    }
}
