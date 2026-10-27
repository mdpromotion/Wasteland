using System.Collections.Generic;
using System.Globalization;

namespace _Project.Features.Items.Domain
{
    public sealed class ToolItemDefinition : ItemDefinition
    {
        public float Durability { get; }
        public float MiningSpeed { get; }
        public float MiningPower { get; }

        public ToolItemDefinition(ItemId id, string displayName, int maxStack, string categoryId, string categoryName,
            float durability, float miningSpeed, float miningPower, IEnumerable<ItemBehavior> behaviors)
            : base(id, displayName, maxStack, categoryId, categoryName, behaviors)
        {
            Durability = durability;
            MiningSpeed = miningSpeed;
            MiningPower = miningPower;
        }

        public override IReadOnlyList<ItemStat> GetInspectableStats()
        {
            var stats = new List<ItemStat>(base.GetInspectableStats())
            {
                new ItemStat("Durability", Format(Durability)),
                new ItemStat("Mining speed", Format(MiningSpeed)),
                new ItemStat("Mining power", Format(MiningPower))
            };
            return stats;
        }

        private static string Format(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    public sealed class FoodItemDefinition : ItemDefinition
    {
        public float Saturation { get; }

        public FoodItemDefinition(ItemId id, string displayName, int maxStack, string categoryId, string categoryName,
            float saturation, IEnumerable<ItemBehavior> behaviors)
            : base(id, displayName, maxStack, categoryId, categoryName, behaviors)
        {
            Saturation = saturation;
        }

        public override IReadOnlyList<ItemStat> GetInspectableStats()
        {
            var stats = new List<ItemStat>(base.GetInspectableStats())
            {
                new ItemStat("Saturation", Saturation.ToString("0.##", CultureInfo.InvariantCulture))
            };
            return stats;
        }
    }
}