using System;
using _Project.Features.Camera.Application;
using _Project.Features.Items.Application;
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
        void ApplyInventoryState(PlayerSaveData data);
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
        private readonly IInventoryStatePersistence _inventoryPersistence;

        public PlayerPersistenceService(
            IPlayerSaveReader reader,
            IPlayerSaveWriter writer,
            IPlayerIdentityProvider identity,
            IPlayerPositionService positionService,
            IFpsPlayerMotor motor,
            IPlayerController player,
            ICameraController camera,
            IWorldSettings worldSettings,
            IInventoryStatePersistence inventoryPersistence)
        {
            _reader = reader;
            _writer = writer;
            _identity = identity;
            _positionService = positionService;
            _motor = motor;
            _player = player;
            _camera = camera;
            _worldSettings = worldSettings;
            _inventoryPersistence = inventoryPersistence
                ?? throw new ArgumentNullException(nameof(inventoryPersistence));
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

            PlayerInventorySaveMapper.ApplyToSaveData(_inventoryPersistence.Capture(), data);

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

        public void ApplyInventoryState(PlayerSaveData data)
        {
            InventorySnapshot snapshot = PlayerInventorySaveMapper.ToSnapshot(data);
            if (snapshot == null)
                return; // Pre-inventory save (schema 1): keep the starting loadout.

            _inventoryPersistence.Apply(snapshot);
        }
    }
}