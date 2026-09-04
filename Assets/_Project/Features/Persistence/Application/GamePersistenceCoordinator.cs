using System;
using Unity.Mathematics;
using _Project.Features.ProceduralWorld.Domain;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using UnityEngine;

namespace _Project.Features.Persistence.Application
{
    public interface IGamePersistenceCoordinator
    {
        void SaveGame();
        bool TryLoadGame();
    }
    
    public sealed class GamePersistenceCoordinator : IGamePersistenceCoordinator
    {
        private readonly IPlayerPersistence _playerPersistence;
        private readonly IChunkPersistence _chunkPersistence;
        private readonly ChunkGrid _chunkGrid;

        public GamePersistenceCoordinator(
            IPlayerPersistence playerPersistence,
            IChunkPersistence chunkPersistence,
            ChunkGrid chunkGrid)
        {
            _playerPersistence = playerPersistence;
            _chunkPersistence = chunkPersistence;
            _chunkGrid = chunkGrid;
        }

        public void SaveGame()
        {
            _chunkPersistence.SaveWorldState();
            _playerPersistence.SavePlayer();
        }

        public bool TryLoadGame()
        {
            if (!_playerPersistence.TryGetSaveData(out var data))
                return false;
            
            int chunkX = (int)Math.Floor(data.X / _chunkGrid.ChunkSizeX);
            int chunkY = (int)Math.Floor(data.Z / _chunkGrid.ChunkSizeZ);
            var targetChunk = new ChunkCoordinate(chunkX, chunkY);
            
            double2 absolutePlayerPos = new double2(data.X, data.Z);
            double2 absoluteChunkOrigin = GenerationSpace.AbsoluteChunkOrigin(
                targetChunk, 
                _chunkGrid.ChunkSizeX, 
                _chunkGrid.ChunkSizeZ);
            
            float2 localOffset = GenerationSpace.LocalOffset(absolutePlayerPos, absoluteChunkOrigin);
            
            Vector3 safeLocalSpawnPosition = new Vector3(localOffset.x, (float)data.Y, localOffset.y);
            
            _chunkPersistence.InitializeOrigin(targetChunk);
            
            _playerPersistence.ApplyPlayerState(safeLocalSpawnPosition, data.Yaw, data.Pitch);

            return true;
        }
    }
}