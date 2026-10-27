using System;

namespace _Project.Features.Items.Domain
{
    public sealed class ItemDefinition
    {
        public ItemId Id { get; }
        public string DisplayName { get; }
        public int MaxStack { get; }
        public bool CanChopTrees { get; }
        public float ToolDamage { get; }
        public bool CanConsume { get; }

        public ItemDefinition(
            ItemId id,
            string displayName,
            int maxStack,
            bool canChopTrees = false,
            float toolDamage = 0f,
            bool canConsume = false)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("An item must have a display name.", nameof(displayName));

            if (maxStack <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxStack));

            if (canChopTrees && toolDamage <= 0f)
                throw new ArgumentOutOfRangeException(nameof(toolDamage));

            Id = id;
            DisplayName = displayName;
            MaxStack = maxStack;
            CanChopTrees = canChopTrees;
            ToolDamage = toolDamage;
            CanConsume = canConsume;
        }
    }
}
