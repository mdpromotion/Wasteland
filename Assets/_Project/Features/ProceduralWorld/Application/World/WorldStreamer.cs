using System;
using System.Collections.Generic;
using _Project.Features.Core.Infrastructure;
using _Project.Features.Graphics.Domain;
using _Project.Features.Player.Domain;
using _Project.Features.ProceduralWorld.Application.Chunks;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using VContainer.Unity;

namespace _Project.Features.ProceduralWorld.Application.World
{
    /// <summary>
    /// Maintains the set of chunks required around the player's current chunk position.
    /// </summary>
    /// <remarks>
    /// The streamer derives the current chunk from the player's world position,
    /// requests a square area determined by the graphics view distance, prioritizes
    /// loading by distance from the center, and unloads chunks outside the required set.
    /// It also triggers world rebasing before refreshing the streaming area.
    /// </remarks>
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

        /// <summary>
        /// Updates the streaming view distance and refreshes the active chunk set.
        /// </summary>
        public void OnGraphicsChanged()
        {
            _viewDistance = _graphicsState.ViewDistance;
            Refresh(_currentCenter);
        }
        
        /// <summary>
        /// Updates the streaming center from the player's current world position and
        /// refreshes the required chunk set when the center changes.
        /// </summary>
        public void Tick()
        {
            ChunkCoordinate center = _chunkGrid.ToChunkCoordinate(_player.Position);

            _worldRebaseService.TryRebase(center);

            if (_initialized && center.Equals(_currentCenter))
            {
                return;
            }
            
            _initialized = true;
            _currentCenter = center;

            Refresh(center);
        }
        
        /// <summary>
        /// Reconciles the currently active chunk set with the chunks required around
        /// the specified center coordinate.
        /// </summary>
        /// <remarks>
        /// Required chunks are queued in distance order. Chunks no longer required are
        /// canceled and unloaded.
        /// </remarks>
        private void Refresh(ChunkCoordinate center)
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

            Utils.SortByDistance(_ordered, center);

            foreach (ChunkCoordinate coordinate in _ordered)
            {
                if (_activeChunks.Contains(coordinate))
                    continue;

                _chunkManager.QueueLoad(coordinate);
            }

            foreach (ChunkCoordinate coordinate in _activeChunks)
            {
                if (_requiredChunks.Contains(coordinate))
                    continue;
                
                _chunkManager.CancelLoad(coordinate);
                
                _chunkManager.Unload(coordinate);
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