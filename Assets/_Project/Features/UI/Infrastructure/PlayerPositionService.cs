using _Project.Features.Player.Domain;
using _Project.Features.ProceduralWorld.Domain;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.World;
using UnityEngine;

namespace _Project.Features.UI.Infrastructure
{
    public interface IPlayerPositionService
    {
        WorldPosition GetPlayerPosition();
        ChunkCoordinate GetCurrentChunkCoordinate();
        
        WorldPosition ToWorldPosition(Vector3 localUnityPosition);
    }

    public class PlayerPositionService : IPlayerPositionService
    {
        private readonly ChunkGrid _chunkGrid;
        private readonly IPlayerReadOnly _player;

        public PlayerPositionService(ChunkGrid chunkGrid, IPlayerReadOnly player)
        {
            _chunkGrid = chunkGrid;
            _player = player;
        }

        public WorldPosition GetPlayerPosition()
            => ToWorldPosition(_player.Position);

        public ChunkCoordinate GetCurrentChunkCoordinate()
            => _chunkGrid.ToChunkCoordinate(_player.Position);

        public WorldPosition ToWorldPosition(Vector3 localUnityPosition)
        {
            ChunkCoordinate currentOrigin = _chunkGrid.OriginCoordinate;

            var originAbs = GenerationSpace.AbsoluteChunkOrigin(
                currentOrigin, _chunkGrid.ChunkSizeX, _chunkGrid.ChunkSizeZ);

            return new WorldPosition(
                originAbs.x + localUnityPosition.x,
                localUnityPosition.y,
                originAbs.y + localUnityPosition.z);
        }
    }
}