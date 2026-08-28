using System;
using UnityEngine;
using VContainer;
using _Project.Features.Player.Domain;
using _Project.Features.ProceduralWorld.Domain.Vegetation;
using _Project.Features.ProceduralWorld.Domain.World;
using _Project.Features.ProceduralWorld.Infrastructure.Vegetation;
using _Project.Features.UI.Infrastructure;

namespace _Project.Features.Player.Infrastructure
{
    public sealed class PlayerBreakableInteractor : MonoBehaviour
    {
        [SerializeField] private float _interval = 1f;
        [SerializeField] private float _rayDistance = 5f;
        [SerializeField] private float _searchRadius = 1f;
        [SerializeField] private LayerMask _raycastMask;

        private IPlayerReadOnly _player;
        private IPlayerPositionService _playerPositionService;
        private IBreakableQuery _breakableQuery;

        private float _timer;

        private bool _hasRaycastHit;

        public event Action<BreakableHit> OnBreakableFound;
        public event Action OnNothingFound;

        [Inject]
        public void Construct(
            IPlayerReadOnly player,
            IPlayerPositionService playerPositionService,
            IBreakableQuery breakableQuery)
        {
            _player = player;
            _playerPositionService = playerPositionService;
            _breakableQuery = breakableQuery;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < _interval)
                return;

            _timer = 0f;
            TryInteract();
        }

        public void TryInteract()
        {
            Vector3 origin = _player.Position;
            Vector3 direction = _player.Forward;
            
            _hasRaycastHit = false;
            
            if (!Physics.Raycast(
                    origin,
                    direction,
                    out RaycastHit rayHit,
                    _rayDistance,
                    _raycastMask))
            {
                Debug.Log("[BreakableInteractor] Raycast did not hit anything.");
                OnNothingFound?.Invoke();
                return;
            }
            
            WorldPosition absoluteHitPosition = _playerPositionService.ToWorldPosition(rayHit.point);

            Debug.Log(
                $"[BreakableInteractor] Raycast hit local=({rayHit.point.x:F3}, {rayHit.point.y:F3}, {rayHit.point.z:F3}) " +
                $"-> absolute=({absoluteHitPosition.X:F3}, {absoluteHitPosition.Y:F3}, {absoluteHitPosition.Z:F3})");
            
            bool found = _breakableQuery.TryFindBreakable(
                absoluteHitPosition,
                _searchRadius,
                out BreakableHit hit);

            if (found)
            {
                OnBreakableFound?.Invoke(hit);
                Debug.Log($"[BreakableInteractor] Breakable found: species={hit.Species}, id={hit.Id}");
            }
            else
            {
                OnNothingFound?.Invoke();
                Debug.Log("[BreakableInteractor] Hit position did not match any breakable instance.");
            }
        }
    }
}