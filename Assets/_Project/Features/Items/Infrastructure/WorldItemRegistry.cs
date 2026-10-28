using System.Collections.Generic;
using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    /// <summary>
    /// Tracks live dropped-item entities by their stable persisted identifier.
    /// Protects the respawn pipeline from duplicate spawns and lets other
    /// modules (e.g. world rebase) iterate the alive entities.
    /// </summary>
    public sealed class WorldItemRegistry
    {
        private readonly Dictionary<ulong, WorldItemPickup> _live = new();

        public bool IsLive(ulong id) => _live.ContainsKey(id);

        public void Register(ulong id, WorldItemPickup pickup)
        {
            if (pickup == null)
                throw new System.ArgumentNullException(nameof(pickup));

            if (_live.ContainsKey(id))
                throw new System.InvalidOperationException(
                    $"A dropped item with id {id} is already alive and must not spawn again.");

            _live[id] = pickup;
        }

        /// <summary>
        /// Removes the registry entry if it still points at <paramref name="owner"/>,
        /// so a respawned entity with a recycled id cannot evict another entity.
        /// </summary>
        public void Release(ulong id, WorldItemPickup owner)
        {
            if (_live.TryGetValue(id, out WorldItemPickup pickup) && pickup == owner)
                _live.Remove(id);
        }

        public IReadOnlyCollection<WorldItemPickup> Live => _live.Values;
    }
}
