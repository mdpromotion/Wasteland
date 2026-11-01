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

            int spawnedFromHit = 0;
            _table.Roll(earnedRolls * _table.DamageStep, itemId =>
            {
                float angle = spawnedFromHit * 2.39996323f + UnityEngine.Random.Range(-0.25f, 0.25f);
                float radius = _table.SpawnSpreadRadius * Mathf.Sqrt(spawnedFromHit + 1f);
                Vector3 lateral = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                Vector3 position = hitPoint + Vector3.up * 0.25f + lateral;
                spawnedFromHit++;

                WorldItemPickup pickup = _spawner.SpawnFresh(itemId, position, Quaternion.identity, Vector3.zero);
                if (pickup == null) return;
                Rigidbody body = pickup.GetComponent<Rigidbody>();
                if (body == null) return;

                Vector2 horizontal = UnityEngine.Random.insideUnitCircle;
                Vector3 impulse = new Vector3(horizontal.x, 0f, horizontal.y) * _table.HorizontalImpulse;
                impulse += Vector3.up * UnityEngine.Random.Range(
                    _table.MinimumUpwardImpulse,
                    _table.MaximumUpwardImpulse);
                body.AddForce(impulse, ForceMode.Impulse);
                body.AddTorque(UnityEngine.Random.insideUnitSphere * _table.SpinImpulse, ForceMode.Impulse);
            });
        }

        private void OnDestroyed(BreakableHit hit) => _damageAccumulators.Remove((hit.Coordinate, hit.Species, hit.Id));
    }
}
