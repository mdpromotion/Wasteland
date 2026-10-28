namespace _Project.Features.Items.Domain
{
    public abstract class ItemBehavior
    {
    }

    public sealed class ConsumableItemBehavior : ItemBehavior
    {
    }

    public sealed class TreeChoppingItemBehavior : ItemBehavior
    {
        public float Damage { get; }

        public TreeChoppingItemBehavior(float damage)
        {
            if (damage <= 0f)
                throw new System.ArgumentOutOfRangeException(nameof(damage));

            Damage = damage;
        }
    }
}
