using _Project.Features.Camera.Infrastructure;
using _Project.Features.Core.Domain;
using _Project.Features.Core.Presentation;
using _Project.Features.Player.Application;
using _Project.Features.Player.Domain;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Features.Camera.Application
{
    public interface ICameraController
    {
        float Pitch { get; }
        void SyncPitch(float pitch);
    }
    
    public class CameraController : ILateTickable, ICameraController
    {
        private readonly ICameraMotor _cameraMotor;
        private readonly PlayerCameraConfig _cameraConfig;
        private readonly IPlayerInputReader _input;
        private readonly IPlayerController _controller;
        private readonly IPlayerStanceState _stance;
        private readonly IGameState _gameState;
        
        private float _pitch;
        private float _currentHeight;
        
        private float _smoothedYaw;
        private float _yawVelocity;
        
        public CameraController(
            ICameraMotor cameraMotor,  
            PlayerCameraConfig cameraConfig,
            IPlayerInputReader input,  
            IPlayerController controller, 
            IPlayerStanceState stance, 
            IGameState gameState )
        {
            _cameraMotor = cameraMotor;
            _cameraConfig = cameraConfig;
            _input = input;
            _controller = controller;
            _stance = stance;
            _gameState = gameState;
            
            _currentHeight = _cameraMotor.GetCurrentHeight();
        }
        
        public void LateTick()
        {
            if (_gameState.Paused)
                return;
            
            UpdateLook();

            UpdateCameraHeight();
        }
        
        public float Pitch => _pitch;

        public void SyncPitch(float pitch)
        {
            _pitch = pitch;
        }
        
        private void UpdateLook()
        {
            Vector2 rawLook = _input.Look * _cameraConfig.sensitivity;

            _controller.SetLookYaw(rawLook.x);

            float y = _cameraConfig.invertY ? rawLook.y : -rawLook.y;

            _pitch = Mathf.Clamp(_pitch + y, -89f, 89f);

            float rawYaw = _controller.LookYaw;

            _smoothedYaw = Mathf.SmoothDampAngle(
                _smoothedYaw,
                rawYaw,
                ref _yawVelocity,
                _cameraConfig.lookSmoothTime,
                Mathf.Infinity,
                Time.deltaTime);

            _cameraMotor.SetRotation(Quaternion.Euler(_pitch, _smoothedYaw, 0f));
        }
        
        private void UpdateCameraHeight()
        {
            float targetHeight = Mathf.Lerp(
                _cameraConfig.standingHeight,
                _cameraConfig.crouchingHeight,
                _stance.CrouchBlend);

            _currentHeight = Mathf.Lerp(
                _currentHeight,
                targetHeight,
                _cameraConfig.heightSmoothSpeed * Time.deltaTime);

            Vector3 offset = _cameraMotor.FollowOffset;
            offset.y = _currentHeight;

            _cameraMotor.SetFollowOffset(offset);
        }
    }
}