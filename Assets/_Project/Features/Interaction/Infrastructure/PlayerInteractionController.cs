using System.Collections.Generic;
using _Project.Features.Core.Presentation;
using _Project.Features.Interaction.Infrastructure;
using _Project.Features.Player.Domain;
using UnityEngine;
using VContainer;

namespace _Project.Features.Player.Infrastructure
{
    public sealed class PlayerInteractionController : MonoBehaviour
    {
        [SerializeField] private float interval = 1f;
        [SerializeField] private float rayDistance = 5f;
        [SerializeField] private LayerMask raycastMask;

        private IPlayerReadOnly _player;
        private IPlayerInputReader _playerInputReader;
        
        private IEnumerable<IHitHandler> _hitHandlers;

        private float _timer;
        
        [Inject]
        public void Construct(
            IPlayerReadOnly player,
            IPlayerInputReader playerInputReader,
            IEnumerable<IHitHandler> hitHandlers)
        {
            _player = player;
            _playerInputReader = playerInputReader;
            _hitHandlers = hitHandlers;
        }

        private void Update()
        {
            _timer += Time.deltaTime; 
            
            if (!_playerInputReader.LeftMouseButtonPressed || _timer < interval) 
                return; 
            
            _timer = 0f;
            TryInteract();
        }
        
        private void TryInteract()
        {
            Vector3 origin = _player.Position;
            Vector3 direction = _player.Forward;
            
            if (!Physics.Raycast(origin, direction, out RaycastHit rayHit, rayDistance, raycastMask))
            {
                return;
            }
            
            foreach (var handler in _hitHandlers)
            {
                if (handler.CanHandle(rayHit))
                {
                    handler.Handle(rayHit);
                    break;
                }
            }
        }
    }
}