using System;
using System.Collections.Generic;
using _Project.Features.Core.Infrastructure;
using _Project.Features.Graphics.Domain;
using _Project.Features.Player.Domain;
using _Project.Features.ProceduralWorld.Application.Chunks;
using _Project.Features.ProceduralWorld.Application.Interfaces;
using _Project.Features.ProceduralWorld.Domain;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using VContainer.Unity;

namespace _Project.Features.ProceduralWorld.Application.World
{
    public class WorldStreamer : IInitializable, ITickable, IDisposable
    {
        private readonly ChunkManager _chunkManager;
        private readonly ChunkGrid _chunkGrid;
        private readonly IPlayerReadOnly _player;
        private readonly GraphicsState _graphicsState;

        private readonly HashSet<ChunkCoordinate> _activeChunks = new();
        private readonly HashSet<ChunkCoordinate> _requiredChunks = new();

        private readonly List<ChunkCoordinate> _ordered = new();
        
        private readonly WorldRebaseService _worldRebaseService;

        private ChunkCoordinate _currentCenter;

        private bool _initialized;
        private int _viewDistance;


        
        public WorldStreamer(
            ChunkManager chunkManager,
            ChunkGrid chunkGrid,
            IPlayerReadOnly player,
            GraphicsState state,
            WorldRebaseService worldRebaseService)
        {
            _chunkManager = chunkManager;
            _chunkGrid = chunkGrid;
            _player = player;
            _graphicsState = state;
            
            _worldRebaseService = worldRebaseService;
        }
        
        public void Initialize()
        {
            _graphicsState.GraphicsChanged += OnGraphicsChanged;
            _viewDistance = _graphicsState.ViewDistance;
        }

        public void OnGraphicsChanged()
        {
            _viewDistance = _graphicsState.ViewDistance;
            Refresh(_currentCenter);
        }


        public void Tick()
        {
            ChunkCoordinate center =
                _chunkGrid.ToChunkCoordinate(_player.Position);

            _worldRebaseService.TryRebase(center);

            if (_initialized && center.Equals(_currentCenter))
            {
                return;
            }
            
            _initialized = true;
            _currentCenter = center;

            Refresh(center);
        }



        private void Refresh(
            ChunkCoordinate center)
        {
            _requiredChunks.Clear();
            _ordered.Clear();

            for (int x = -_viewDistance; x <= _viewDistance; x++)
            {
                for (int y = -_viewDistance; y <= _viewDistance; y++)
                {
                    ChunkCoordinate coordinate =
                        new ChunkCoordinate(
                            center.X + x,
                            center.Y + y);

                    _requiredChunks.Add(coordinate);
                    _ordered.Add(coordinate);
                }
            }

            Utils.SortByDistance(
                _ordered,
                center);

            foreach (ChunkCoordinate coordinate in _ordered)
            {
                if (_activeChunks.Contains(coordinate))
                    continue;

                _chunkManager.QueueLoad(
                    coordinate);
            }

            foreach (ChunkCoordinate coordinate in _activeChunks)
            {
                if (_requiredChunks.Contains(coordinate))
                    continue;


                _chunkManager.CancelLoad(
                    coordinate);


                _chunkManager.Unload(
                    coordinate);
            }

            _activeChunks.Clear();

            foreach (ChunkCoordinate coordinate in _requiredChunks)
            {
                _activeChunks.Add(coordinate);
            }
        }

        public void Dispose()
        {
            _graphicsState.GraphicsChanged -= OnGraphicsChanged;
        }
    }
}