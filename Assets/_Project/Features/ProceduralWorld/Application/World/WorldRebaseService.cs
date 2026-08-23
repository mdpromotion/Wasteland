using System.Collections.Generic;
using System.Linq;
using _Project.Features.ProceduralWorld.Application.Chunks;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.World;
using _Project.Features.ProceduralWorld.Infrastructure.Chunks;
using _Project.Features.ProceduralWorld.Presentation.World;
using UnityEngine;

namespace _Project.Features.ProceduralWorld.Application.World
{
    public sealed class WorldRebaseService
    {
        private readonly ChunkGrid _grid;
        private readonly IChunkLookup _repository;
        private readonly WorldRebaseSettings _settings;
        private readonly IWorldRebaseParticipant[] _participants;
        
        private readonly IWorldRebaseApplier _applier;

        public WorldRebaseService(
            ChunkGrid grid,
            IChunkLookup repository,
            WorldRebaseSettings settings,
            IEnumerable<IWorldRebaseParticipant> participants,
            IWorldRebaseApplier applier)
        {
            _grid = grid;
            _repository = repository;
            _settings = settings;
            _applier = applier;
            
            _participants = participants.OrderBy(p => p.Order).ToArray();
        }

        public void TryRebase(ChunkCoordinate center)
        {
            ChunkCoordinate origin = _grid.OriginCoordinate;

            int dx = Mathf.Abs(center.X - origin.X);
            int dy = Mathf.Abs(center.Y - origin.Y);
            
            if (dx < _settings.ThresholdChunks &&
                dy < _settings.ThresholdChunks)
            {
                return;
            }

            Vector2 oldOffset = _grid.ToWorldOffset(center);
            Vector3 delta = new Vector3(-oldOffset.x, 0f, -oldOffset.y);
            
            foreach (ChunkInstance chunk in _repository.All)
            {
                _applier.MoveChunkTo(chunk, delta);
            }
            
            for (int i = 0; i < _participants.Length; i++)
            {
                _participants[i].OnWorldRebased(delta);
            }
            
            _applier.SyncTransforms();

            _grid.SetOriginCoordinate(center);
        }
    }
}