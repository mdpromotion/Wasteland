using System;
using _Project.Features.Player.Infrastructure;
using _Project.Features.ProceduralWorld.Application.Chunks;
using _Project.Features.ProceduralWorld.Application.Persistence;
using _Project.Features.ProceduralWorld.Domain.Vegetation;
using _Project.Features.ProceduralWorld.Domain.World;
using _Project.Features.ProceduralWorld.Infrastructure.Chunks;
using _Project.Features.ProceduralWorld.Infrastructure.Vegetation;
using _Project.Features.ProceduralWorld.Presentation.Vegetation;
using _Project.Features.Sound.Presentation;
using _Project.Features.UI.Infrastructure;
using UnityEngine;

namespace _Project.Features.Interaction.Infrastructure
{
    public sealed class BreakableHitHandler : IHitHandler
    {
        private readonly IPlayerPositionService _playerPositionService;
        private readonly IBreakableQuery _breakableQuery;
        private readonly IBreakableTreeHealthService _healthService;
        private readonly IChunkLookup _chunkLookup;
        private readonly VegetationApplier _vegetationApplier;
        private readonly IChunkMutationTracker _mutationTracker;
        private readonly ISoundService _soundService;
        private readonly PlayerSoundSet _playerSoundSet;

        private readonly int _breakableLayer;
        private readonly float _searchRadius;

        public event Action<BreakableHit> OnBreakableFound;
        public event Action OnNothingFound;
        public event Action<BreakableHit> OnBreakableDestroyed;

        public BreakableHitHandler(
            IPlayerPositionService playerPositionService,
            IBreakableQuery breakableQuery,
            IBreakableTreeHealthService healthService,
            IChunkLookup chunkLookup,
            VegetationApplier vegetationApplier,
            IChunkMutationTracker mutationTracker,
            ISoundService soundService,
            PlayerSoundSet playerSoundSet)
        {
            _playerPositionService = playerPositionService;
            _breakableQuery = breakableQuery;
            _healthService = healthService;
            _chunkLookup = chunkLookup;
            _vegetationApplier = vegetationApplier;
            _mutationTracker = mutationTracker;
            _soundService = soundService;
            _playerSoundSet = playerSoundSet;
            _searchRadius = 5f;

            _breakableLayer = LayerMask.NameToLayer("Ground");
        }

        public bool CanHandle(RaycastHit hit) => hit.collider.gameObject.layer == _breakableLayer;

        public void Handle(RaycastHit hit, float damage)
        {
            WorldPosition absoluteHitPosition = _playerPositionService.ToWorldPosition(hit.point);

            if (!_breakableQuery.TryFindBreakable(absoluteHitPosition, _searchRadius, out BreakableHit breakableHit))
            {
                OnNothingFound?.Invoke();
                return;
            }

            OnBreakableFound?.Invoke(breakableHit);
            _soundService.PlayAt(_playerSoundSet.TreeChop, hit.point);
            _vegetationApplier.ShowTreeImpact(
                breakableHit.Species,
                breakableHit.Id,
                hit.point,
                hit.normal);

            bool destroyed = _healthService.ApplyDamage(breakableHit, damage);
            if (!destroyed)
                return;

            if (!_chunkLookup.TryGet(breakableHit.Coordinate, out ChunkInstance chunk) || chunk.Vegetation == null)
                return;

            if (!chunk.Vegetation.TryRemoveInstance(breakableHit.Species, breakableHit.Id))
                return;

            _mutationTracker.RecordVegetationRemoved(breakableHit.Coordinate, breakableHit.Id);
            _vegetationApplier.Apply(chunk.Vegetation, chunk.Terrain);
            _soundService.PlayAt(_playerSoundSet.TreeFall, hit.point);
            OnBreakableDestroyed?.Invoke(breakableHit);
        }
    }
}