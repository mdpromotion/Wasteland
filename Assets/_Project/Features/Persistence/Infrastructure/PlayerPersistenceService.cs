using System;
using _Project.Features.Camera.Application;
using _Project.Features.Persistence.Domain;
using _Project.Features.Player.Application;
using _Project.Features.Player.Domain;
using _Project.Features.Player.Presentation;
using _Project.Features.ProceduralWorld.Domain.World;
using _Project.Features.UI.Infrastructure;
using UnityEngine;

namespace _Project.Features.Persistence.Application
{
    public interface IPlayerPersistence
    {
        void SavePlayer();
        bool TryGetSaveData(out PlayerSaveData data);
        void ApplyPlayerState(Vector3 localPosition, float yaw, float pitch);
    }

    public sealed class PlayerPersistenceService : IPlayerPersistence
    {
        private readonly IPlayerSaveReader _reader;
        private readonly IPlayerSaveWriter _writer;
        private readonly IPlayerIdentityProvider _identity;
        private readonly IPlayerPositionService _positionService;
        private readonly IFpsPlayerMotor _motor;
        private readonly IPlayerController _player;
        private readonly ICameraController _camera;
        private readonly IWorldSettings _worldSettings;

        public PlayerPersistenceService(
            IPlayerSaveReader reader,
            IPlayerSaveWriter writer,
            IPlayerIdentityProvider identity,
            IPlayerPositionService positionService,
            IFpsPlayerMotor motor,
            IPlayerController player,
            ICameraController camera,
            IWorldSettings worldSettings)
        {
            _reader = reader;
            _writer = writer;
            _identity = identity;
            _positionService = positionService;
            _motor = motor;
            _player = player;
            _camera = camera;
            _worldSettings = worldSettings;
        }

        public void SavePlayer()
        {
            var playerId = _identity.GetPlayerId();
            var worldPos = _positionService.ToWorldPosition(_player.LastKnownPosition);

            var data = new PlayerSaveData
            {
                PlayerId = playerId,
                X = worldPos.X,
                Y = worldPos.Y,
                Z = worldPos.Z,
                Yaw = _player.LookYaw,
                Pitch = _camera.Pitch,
                SavedAtTicks = DateTime.UtcNow.Ticks
            };

            _writer.SavePlayer(_worldSettings.Name, playerId, data);
        }

        public bool TryGetSaveData(out PlayerSaveData data)
        {
            var playerId = _identity.GetPlayerId();
            return _reader.TryReadPlayer(_worldSettings.Name, playerId, out data);
        }

        public void ApplyPlayerState(Vector3 localPosition, float yaw, float pitch)
        {
            _motor.TeleportToPosition(localPosition);
            _motor.SetRotation(Quaternion.Euler(0f, yaw, 0f));

            _player.SyncYaw(yaw);
            _camera.SyncPitch(pitch);
        }
    }
}