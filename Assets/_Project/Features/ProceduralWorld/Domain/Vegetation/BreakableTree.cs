namespace _Project.Features.ProceduralWorld.Domain.Vegetation
{
    /// <summary>
    /// Identity and health of a single breakable vegetation instance.
    /// Tracked purely by Id — never bound to a position.
    /// </summary>
    public sealed class BreakableTree
    {
        public ulong Id { get; }
        public float Health { get; private set; }

        public BreakableTree(ulong id, float health)
        {
            Id = id;
            Health = health;
        }

        /// <summary>
        /// Applies damage. Returns true once health has dropped to zero or below.
        /// </summary>
        public bool ApplyDamage(float damage)
        {
            Health -= damage;
            return Health <= 0f;
        }
    }
}