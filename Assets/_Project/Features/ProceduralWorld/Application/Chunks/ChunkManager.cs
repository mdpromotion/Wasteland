using System;
using System.Collections.Generic;
using _Project.Features.ProceduralWorld.Application.Chunks.Generation;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Infrastructure.Chunks;
using _Project.Features.ProceduralWorld.Infrastructure.Interfaces;
using _Project.Features.ProceduralWorld.Presentation.Landscape;
using VContainer.Unity;

namespace _Project.Features.ProceduralWorld.Application.Chunks
{
    public interface IChunkManager
    {
        bool IsReady { get; }
    }

    /// <summary>
    /// Coordinates chunk generation, application, storage, neighbour connections,
    /// and unloading for the procedural world.
    /// </summary>
    /// <remarks>
    /// The manager does not perform generation itself. It schedules requests through
    /// <see cref="ChunkGenerationScheduler"/>, applies completed results, and keeps
    /// generated chunks synchronized with the chunk repository.
    /// </remarks>
    public class ChunkManager : IChunkManager, ITickable, IDisposable
    {
        private readonly ChunkGenerationScheduler _scheduler;
        private readonly ChunkRepository _repository;

        private readonly ILandscapeFactory _factory;
        private readonly ChunkNeighborConnector _neighborConnector;
        
        private readonly HashSet<ChunkCoordinate> _loading = new();
        
        private readonly Action<ChunkGenerationResult> _applyAction;
        private readonly Action<ChunkCoordinate> _completedAction;

        public bool IsReady { get; private set; }
        

        public ChunkManager(
            ChunkGenerationScheduler scheduler,
            ChunkRepository repository,
            ILandscapeApplier applier,
            ILandscapeFactory factory,
            ChunkNeighborConnector neighborConnector)
        {
            _scheduler = scheduler;
            _repository = repository;
            _factory = factory;
            _neighborConnector = neighborConnector;
            
            _applyAction = result =>
            {
                applier.Apply(result);

                result.Dispose();
            };
 
            _completedAction = FinishLoading;
        }
        
        /// <summary>
        /// Advances generation scheduling and applies completed generation results.
        /// </summary>
        public void Tick()
        {
            _scheduler.Tick(_applyAction, _completedAction);
        }
        
        public void Dispose()
        {
            _scheduler.CompleteAll();

            _repository.Dispose();
        }

        /// <summary>
        /// Queues a chunk for generation unless it is already loaded or being generated.
        /// </summary>
        /// <param name="coordinate">Logical coordinate of the requested chunk.</param>
        public void QueueLoad(
            ChunkCoordinate coordinate)
        {
            if(_repository.Contains(coordinate))
                return;

            if(!_loading.Add(coordinate))
                return;

            _scheduler.Enqueue( new ChunkGenerationRequest(coordinate, 257));
        }
        
        /// <summary>
        /// Cancels a queued or currently running generation request for the specified chunk.
        /// </summary>
        public void CancelLoad(ChunkCoordinate coordinate)
        {
            _loading.Remove(coordinate);


            _scheduler.Cancel(
                coordinate);
        }
        
        private void FinishLoading(ChunkCoordinate coordinate)
        {
            _loading.Remove(
                coordinate);

            if (!IsReady && _repository.Contains(coordinate))
            {
                IsReady = true;
            }
        }
        
        /// <summary>
        /// Removes a loaded chunk, disconnects its terrain neighbours, releases its generated
        /// data, and returns its Unity terrain representation to the landscape factory.
        /// </summary>
        public void Unload(ChunkCoordinate coordinate)
        {
            if (!_repository.TryGet(coordinate, out ChunkInstance chunk))
                return;
 
            _neighborConnector.Disconnect(_repository, coordinate);
 
            _repository.Remove(coordinate);
 
            chunk.Landscape.Dispose();
            chunk.Hydrology.Dispose();
 
            _factory.Release(chunk.Terrain);
        }

    }
}