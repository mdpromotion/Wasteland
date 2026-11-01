using System;
using _Project.Features.Interaction.Infrastructure;
using _Project.Features.Items.Domain;
using _Project.Features.Items.Infrastructure;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Features.Items.Application
{
    /// <summary>
    /// Spawns a wood drop at the position of a destroyed breakable tree.
    /// Bridges the vegetation breakable pipeline into the item system
    /// without coupling the hit handler to the item domain.
    /// </summary>
    public sealed class TreeFallWoodDropper : IInitializable, IDisposable
    {
        private const float DropPopVelocity = 1.5f;
        private const float DropLiftHeight = 1f;

        private readonly BreakableHitHandler _hitHandler;
        private readonly IWorldItemSpawner _spawner;

        public TreeFallWoodDropper(BreakableHitHandler hitHandler, IWorldItemSpawner spawner)
        {
            _hitHandler = hitHandler ?? throw new ArgumentNullException(nameof(hitHandler));
            _spawner = spawner ?? throw new ArgumentNullException(nameof(spawner));
        }

        public void Initialize()
        {
            _hitHandler.OnBreakableDestroyed += OnTreeDestroyed;
        }

        public void Dispose()
        {
            _hitHandler.OnBreakableDestroyed -= OnTreeDestroyed;
        }

        private void OnTreeDestroyed(BreakableHit hit)
        {
            Vector3 position = new Vector3(
                hit.WorldPosition.x,
                hit.WorldPosition.y,
                hit.WorldPosition.z) + Vector3.up * DropLiftHeight;

            _spawner.SpawnFresh(
                ItemId.Wood,
                position,
                Quaternion.identity,
                Vector3.up * DropPopVelocity);
        }
    }
}
