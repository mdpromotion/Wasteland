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
        bool TryLoadPlayer();
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

        public bool TryLoadPlayer()
        {
            var playerId = _identity.GetPlayerId();

            if (!_reader.TryReadPlayer(_worldSettings.Name, playerId, out var data))
                return false;

            var spawnPosition = new Vector3((float)data.X, data.Y, (float)data.Z);

            _motor.TeleportToPosition(spawnPosition);
            _motor.SetRotation(Quaternion.Euler(0f, data.Yaw, 0f));

            _player.SyncYaw(data.Yaw);
            _camera.SyncPitch(data.Pitch);

            return true;
        }
    }
}