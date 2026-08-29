using System;
using _Project.Features.Interaction.Infrastructure;
using _Project.Features.ProceduralWorld.Application.Chunks;
using _Project.Features.ProceduralWorld.Domain.Vegetation;
using _Project.Features.ProceduralWorld.Domain.World;
using _Project.Features.ProceduralWorld.Infrastructure.Chunks;
using _Project.Features.ProceduralWorld.Infrastructure.Vegetation;
using _Project.Features.ProceduralWorld.Presentation.Vegetation;
using _Project.Features.UI.Infrastructure;
using UnityEngine;

public sealed class BreakableHitHandler : IHitHandler
{
    private readonly IPlayerPositionService _playerPositionService;
    private readonly IBreakableQuery _breakableQuery;
    private readonly IBreakableTreeHealthService _healthService;
    private readonly IChunkLookup _chunkLookup;
    private readonly VegetationApplier _vegetationApplier;

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
        VegetationApplier vegetationApplier)
    {
        _playerPositionService = playerPositionService;
        _breakableQuery = breakableQuery;
        _healthService = healthService;
        _chunkLookup = chunkLookup;
        _vegetationApplier = vegetationApplier;
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

        bool destroyed = _healthService.ApplyDamage(breakableHit, damage);
        if (!destroyed)
            return;

        if (!_chunkLookup.TryGet(breakableHit.Coordinate, out ChunkInstance chunk) || chunk.Vegetation == null)
            return;

        if (!chunk.Vegetation.TryRemoveInstance(breakableHit.Species, breakableHit.Id))
            return; // уже удалено — например, повторный удар пришёлся на кадр после срубки

        _vegetationApplier.Apply(chunk.Vegetation, chunk.Terrain);

        OnBreakableDestroyed?.Invoke(breakableHit);
    }
}