using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Features.Core.Domain;
using _Project.Features.Core.Presentation;
using _Project.Features.Player.Domain;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

namespace _Project.Features.Interaction.Infrastructure
{
    public interface IPlayerInteractionController
    {
        /// <summary>
        /// Raised when the player performs an interaction.
        /// </summary>
        /// <param name="interval">
        /// The cooldown interval before the next interaction can be performed.
        /// </param>
        event Action<float> PlayerInteracted;
    }
    
    public sealed class PlayerInteractionController : MonoBehaviour, IPlayerInteractionController
    {
        [SerializeField] private float interval = 1f;
        [SerializeField] private float rayDistance = 5f;
        [SerializeField] private LayerMask raycastMask;

        private IGameState _gameState;
        private IPlayerReadOnly _player;
        private IPlayerInputReader _playerInputReader;
        
        private IEnumerable<IHitHandler> _hitHandlers;

        private float _timer;

        private const float Damage = 1f; // TODO: Refactor this into Inventory/Item feature. 
        
        public event Action<float> PlayerInteracted; 
        
        [Inject]
        public void Construct(
            IGameState gameState,
            IPlayerReadOnly player,
            IPlayerInputReader playerInputReader,
            IEnumerable<IHitHandler> hitHandlers)
        {
            _gameState = gameState;
            _player = player;
            _playerInputReader = playerInputReader;
            _hitHandlers = hitHandlers;
        }

        private void Update()
        {
            if (_gameState.Paused)
                return;
            
            _timer += Time.deltaTime; 
            
            if (!_playerInputReader.LeftMouseButtonPressed || _timer < interval) 
                return; 
            
            _timer = 0f;
            TryInteract();
        }
        
        private void TryInteract()
        {
            PlayerInteracted?.Invoke(interval);
            
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
                    handler.Handle(rayHit, Damage);
                    break;
                }
            }
        }
    }
}