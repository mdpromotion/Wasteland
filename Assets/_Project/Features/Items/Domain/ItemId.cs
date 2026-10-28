using System;

namespace _Project.Features.Items.Domain
{
    public readonly struct ItemId : IEquatable<ItemId>
    {
        public static readonly ItemId IronAxe = new ItemId("iron_axe");
        public static readonly ItemId Wood = new ItemId("wood");
        public static readonly ItemId Apple = new ItemId("apple");

        public string Value { get; }

        public ItemId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("An item ID cannot be empty.", nameof(value));

            Value = value.Trim();
        }

        public bool Equals(ItemId other) =>
            string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is ItemId other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value ?? string.Empty;

        public static bool operator ==(ItemId left, ItemId right) => left.Equals(right);
        public static bool operator !=(ItemId left, ItemId right) => !left.Equals(right);
    }
}
