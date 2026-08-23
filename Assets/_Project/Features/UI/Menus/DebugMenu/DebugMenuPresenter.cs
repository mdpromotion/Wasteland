using _Project.Features.Core.Infrastructure;
using _Project.Features.UI.Infrastructure;
using UnityEngine;
using VContainer;

namespace _Project.Features.UI.Menus.DebugMenu
{
    public class DebugMenuPresenter : MonoBehaviour
    { 
        [SerializeField] private DebugTextView positionTextView;
        [SerializeField] private DebugTextView fpsTextView;
        
        private IFPSCounter _fpsCounter;
        private IPlayerPositionService _playerPosition;

        private const string FpsCounterName = "FPS";
        
        [Inject]
        public void Construct(IFPSCounter fpsCounter, IPlayerPositionService playerPosition)
        {
            _fpsCounter = fpsCounter;
            _playerPosition = playerPosition;
        }


        private void Update()
        {
            UpdatePositionText();
            UpdateFpsCounter();
        }
        
        private void UpdatePositionText()
        {
            if (_playerPosition == null) return;
            
            var worldPosition = _playerPosition.GetPlayerPosition();
            var currentChunkCoordinate = _playerPosition.GetCurrentChunkCoordinate();
            
            var text = $"X: {worldPosition.X:F0}, Y: {worldPosition.Y:F0}, Z: {worldPosition.Z:F0} ({currentChunkCoordinate.X}, {currentChunkCoordinate.Y})";
            positionTextView.ChangeText(text);
        }

        private void UpdateFpsCounter()
        {
            if (_fpsCounter == null) return;
            
            var text = $"{FpsCounterName}: {_fpsCounter.CurrentFps:F0}";
            fpsTextView.ChangeText(text);
        }
        
    }
}
