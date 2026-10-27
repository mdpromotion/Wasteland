using System;
using System.Collections.Generic;
using _Project.Features.Core.Domain;
using _Project.Features.Core.Presentation;
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
        private IPlayerInputReader _playerInputReader;
        private IEnumerable<IHitHandler> _hitHandlers;
        private UnityEngine.Camera _interactionCamera;

        private float _timer;

        private const float Damage = 1f; // TODO: Refactor this into Inventory/Item feature. 
        
        public event Action<float> PlayerInteracted; 
        
        [Inject]
        public void Construct(
            IGameState gameState,
            IPlayerInputReader playerInputReader,
            IEnumerable<IHitHandler> hitHandlers)
        {
            _gameState = gameState;
            _playerInputReader = playerInputReader;
            _hitHandlers = hitHandlers;
            _interactionCamera = UnityEngine.Camera.main;
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

            if (!_interactionCamera)
                _interactionCamera = UnityEngine.Camera.main;

            if (!_interactionCamera)
                return;

            Ray interactionRay = new Ray(
                _interactionCamera.transform.position,
                _interactionCamera.transform.forward);

            if (!Physics.Raycast(interactionRay, out RaycastHit rayHit, rayDistance, raycastMask))
                return;
            
            foreach (var handler in _hitHandlers)
            {
                if (!handler.CanHandle(rayHit))
                    continue;

                handler.Handle(rayHit, Damage);
                break;
            }
        }
    }
}