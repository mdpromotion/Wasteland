using _Project.Features.Items.Domain;
using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    public abstract class ItemBehaviorAsset : ScriptableObject
    {
        public abstract ItemBehavior CreateBehavior();
    }
}
