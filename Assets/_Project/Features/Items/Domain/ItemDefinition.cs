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

        /// <summary>
        /// Damage dealt with empty hands (no held item) or by items without chopping behavior.
        /// </summary>
        public const float FistDamage = 1f;

        public bool CanChopTrees => GetBehavior<TreeChoppingItemBehavior>() != null;
        public float ToolDamage => GetBehavior<TreeChoppingItemBehavior>()?.Damage ?? 0f;
        public bool CanConsume => GetBehavior<ConsumableItemBehavior>() != null;

        /// <summary>
        /// Damage dealt to breakable trees with this item. Sourced exclusively from the
        /// <see cref="TreeChoppingItemBehavior"/> so there is a single place defining it.
        /// </summary>
        public float GetTreeDamage() => GetBehavior<TreeChoppingItemBehavior>()?.Damage ?? FistDamage;

        public ItemDefinition(ItemId id, string displayName, int maxStack, IEnumerable<ItemBehavior> behaviors = null)
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

        public T GetBehavior<T>() where T : ItemBehavior
        {
            for (int i = 0; i < Behaviors.Count; i++)
                if (Behaviors[i] is T behavior)
                    return behavior;
            return null;
        }

        private IReadOnlyList<ItemStat> _cachedStats;

        /// <summary>
        /// UI-facing stat rows. The list is immutable per definition and cached, so
        /// HUD re-renders don't rebuild it on every inventory change.
        /// </summary>
        public IReadOnlyList<ItemStat> GetInspectableStats() =>
            _cachedStats ??= CreateInspectableStats();

        protected virtual IReadOnlyList<ItemStat> CreateInspectableStats()
        {
            return new[]
            {
                new ItemStat("Category", string.IsNullOrEmpty(CategoryName) ? "Uncategorized" : CategoryName),
                new ItemStat("Technical ID", Id.Value),
                new ItemStat("Max stack", MaxStack.ToString(CultureInfo.InvariantCulture))
            };
        }

    }
}
