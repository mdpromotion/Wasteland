using System.Collections.Generic;
using _Project.Features.ProceduralWorld.Domain.Chunks;

namespace _Project.Features.ProceduralWorld.Domain.Vegetation
{
    public interface IBreakableTreeHealthService
    {
        bool ApplyDamage(BreakableHit hit, float damage);
    }

    public sealed class BreakableTreeHealthService : IBreakableTreeHealthService
    {
        private const float DefaultHealth = 3f;

        private readonly Dictionary<(ChunkCoordinate, VegetationSpeciesType, ulong), BreakableTree> _states = new();

        public bool ApplyDamage(BreakableHit hit, float damage)
        {
            var key = (hit.Coordinate, hit.Species, hit.Id);

            if (!_states.TryGetValue(key, out BreakableTree tree))
            {
                tree = new BreakableTree(hit.Id, DefaultHealth);
                _states[key] = tree;
            }

            bool destroyed = tree.ApplyDamage(damage);

            if (destroyed)
                _states.Remove(key);

            return destroyed;
        }
    }
}