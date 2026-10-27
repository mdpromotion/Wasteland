using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace _Project.Features.Items.Domain
{
    public class ItemDefinition
    {
        public ItemId Id { get; }
        public string DisplayName { get; }
        public int MaxStack { get; }
        public string CategoryId { get; }
        public string CategoryName { get; }
        public IReadOnlyList<ItemBehavior> Behaviors { get; }

        public bool CanChopTrees => GetBehavior<TreeChoppingItemBehavior>() != null;
        public float ToolDamage => GetBehavior<TreeChoppingItemBehavior>()?.Damage ?? 0f;
        public bool CanConsume => GetBehavior<ConsumableItemBehavior>() != null;

        public ItemDefinition(ItemId id, string displayName, int maxStack, IEnumerable<ItemBehavior> behaviors)
            : this(id, displayName, maxStack, string.Empty, string.Empty, behaviors)
        {
        }

        public ItemDefinition(
            ItemId id,
            string displayName,
            int maxStack,
            string categoryId,
            string categoryName,
            IEnumerable<ItemBehavior> behaviors)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("An item must have a display name.", nameof(displayName));
            if (maxStack <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxStack));

            Id = id;
            DisplayName = displayName;
            MaxStack = maxStack;
            CategoryId = categoryId ?? string.Empty;
            CategoryName = categoryName ?? string.Empty;
            Behaviors = (behaviors ?? Array.Empty<ItemBehavior>()).ToArray();
            if (Behaviors.Any(behavior => behavior == null))
                throw new ArgumentException("An item's behaviors cannot contain null entries.", nameof(behaviors));
        }

        public ItemDefinition(ItemId id, string displayName, int maxStack,
            bool canChopTrees = false, float toolDamage = 0f, bool canConsume = false)
            : this(id, displayName, maxStack, CreateLegacyBehaviors(canChopTrees, toolDamage, canConsume))
        {
        }

        public T GetBehavior<T>() where T : ItemBehavior
        {
            for (int i = 0; i < Behaviors.Count; i++)
                if (Behaviors[i] is T behavior)
                    return behavior;
            return null;
        }

        public virtual float GetTreeDamage() => 1f;

        public virtual IReadOnlyList<ItemStat> GetInspectableStats()
        {
            return new[]
            {
                new ItemStat("Category", string.IsNullOrEmpty(CategoryName) ? "Uncategorized" : CategoryName),
                new ItemStat("Technical ID", Id.Value),
                new ItemStat("Max stack", MaxStack.ToString(CultureInfo.InvariantCulture))
            };
        }

        private static IEnumerable<ItemBehavior> CreateLegacyBehaviors(bool canChopTrees, float toolDamage, bool canConsume)
        {
            var behaviors = new List<ItemBehavior>();
            if (canChopTrees)
                behaviors.Add(new TreeChoppingItemBehavior(toolDamage));
            if (canConsume)
                behaviors.Add(new ConsumableItemBehavior());
            return behaviors;
        }
    }
}
