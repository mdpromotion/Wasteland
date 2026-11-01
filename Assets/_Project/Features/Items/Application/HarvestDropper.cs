using System;
using System.Collections.Generic;
using _Project.Features.Interaction.Infrastructure;
using _Project.Features.Items.Infrastructure;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Vegetation;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Features.Items.Application
{
    public sealed class HarvestDropper : IInitializable, IDisposable
    {
        private readonly BreakableHitHandler _hitHandler;
        private readonly HarvestDropTable _table;
        private readonly IWorldItemSpawner _spawner;
        private readonly Dictionary<(ChunkCoordinate, VegetationSpeciesType, ulong), DamageStepAccumulator> _damageAccumulators = new();
        public HarvestDropper(BreakableHitHandler hitHandler, HarvestDropTable table, IWorldItemSpawner spawner)
        {
            _hitHandler = hitHandler ?? throw new ArgumentNullException(nameof(hitHandler));
            _table = table ?? throw new ArgumentNullException(nameof(table));
            _spawner = spawner ?? throw new ArgumentNullException(nameof(spawner));
        }

        public void Initialize()
        {
            _hitHandler.OnBreakableDamaged += OnDamaged;
            _hitHandler.OnBreakableDestroyed += OnDestroyed;
        }

        public void Dispose()
        {
            _hitHandler.OnBreakableDamaged -= OnDamaged;
            _hitHandler.OnBreakableDestroyed -= OnDestroyed;
        }

        private void OnDamaged(BreakableHit hit, float damage, Vector3 hitPoint)
        {
            var key = (hit.Coordinate, hit.Species, hit.Id);
            if (!_damageAccumulators.TryGetValue(key, out DamageStepAccumulator accumulator))
                _damageAccumulators[key] = accumulator = new DamageStepAccumulator();
            int earnedRolls = accumulator.Add(damage, _table.DamageStep);
            if (earnedRolls == 0)
                return;

            Vector3 position = hitPoint + Vector3.up * 0.25f;
            _table.Roll(earnedRolls * _table.DamageStep, itemId =>
            {
                WorldItemPickup pickup = _spawner.SpawnFresh(itemId, position, Quaternion.identity, Vector3.zero);
                if (pickup == null) return;
                Rigidbody body = pickup.GetComponent<Rigidbody>();
                if (body != null) body.AddForce((Vector3.up + UnityEngine.Random.insideUnitSphere * 0.25f) * 2f, ForceMode.Impulse);
            });
        }

        private void OnDestroyed(BreakableHit hit) => _damageAccumulators.Remove((hit.Coordinate, hit.Species, hit.Id));
    }
}
