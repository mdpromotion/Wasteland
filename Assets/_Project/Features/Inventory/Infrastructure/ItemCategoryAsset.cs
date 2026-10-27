using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    [CreateAssetMenu(menuName = "Project/Items/Category", fileName = "ItemCategory")]
    public sealed class ItemCategoryAsset : ScriptableObject
    {
        [SerializeField] private string technicalId;
        [SerializeField] private string displayName;

        public string TechnicalId => technicalId;
        public string DisplayName => displayName;
    }
}