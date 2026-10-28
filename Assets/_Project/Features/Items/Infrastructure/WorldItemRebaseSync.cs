using _Project.Features.ProceduralWorld.Application.World;
using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    /// <summary>
    /// Keeps live dropped-item entities glued to the terrain when the world
    /// floating origin is rebased. Chunk terrains are moved via
    /// <see cref="IWorldRebaseApplier"/>; these entities follow them here.
    /// </summary>
    public sealed class WorldItemRebaseSync : IWorldRebaseParticipant
    {
        private readonly WorldItemRegistry _registry;

        public WorldItemRebaseSync(WorldItemRegistry registry)
        {
            _registry = registry;
        }

        public int Order => 200;

        public void OnWorldRebased(Vector3 delta)
        {
            foreach (WorldItemPickup pickup in _registry.Live)
            {
                if (pickup == null)
                    continue;

                pickup.transform.position += delta;
            }
        }
    }
}
