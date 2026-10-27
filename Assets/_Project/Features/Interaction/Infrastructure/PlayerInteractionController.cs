using System;
using System.Collections.Generic;
using _Project.Features.Core.Domain;
using _Project.Features.Core.Presentation;
using _Project.Features.Items.Domain;
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
        private Inventory _inventory;

        private float _timer;

        public event Action<float> PlayerInteracted; 
        
        [Inject]
        public void Construct(
            IGameState gameState,
            IPlayerInputReader playerInputReader,
            IEnumerable<IHitHandler> hitHandlers,
            Inventory inventory)
        {
            _gameState = gameState;
            _playerInputReader = playerInputReader;
            _hitHandlers = hitHandlers;
            _inventory = inventory;
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

            ItemDefinition selectedItem = _inventory.SelectedItem;
            if (selectedItem == null || !selectedItem.CanChopTrees)
                return;

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

                handler.Handle(rayHit, selectedItem.ToolDamage);
                break;
            }
        }
    }
}
