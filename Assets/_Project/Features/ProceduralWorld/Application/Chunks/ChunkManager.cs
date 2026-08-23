using System;
using System.Collections.Generic;
using _Project.Features.ProceduralWorld.Application.Chunks.Generation;
using _Project.Features.ProceduralWorld.Application.Interfaces;
using _Project.Features.ProceduralWorld.Application.Landscape;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Infrastructure.Chunks;
using _Project.Features.ProceduralWorld.Infrastructure.Interfaces;
using VContainer.Unity;

namespace _Project.Features.ProceduralWorld.Application.Chunks
{
    public interface IChunkManager
    {
        bool IsReady { get; }
    }
    
    public class ChunkManager : IChunkManager, ITickable, IDisposable
    {
        private readonly ChunkGenerationScheduler _scheduler;
        private readonly ChunkRepository _repository;

        private readonly ILandscapeFactory _factory;
        private readonly IChunkNeighborConnector _neighborConnector;
        
        private readonly HashSet<ChunkCoordinate> _loading = new();
        
        private readonly Action<ChunkGenerationResult> _applyAction;
        private readonly Action<ChunkCoordinate> _completedAction;

        public bool IsReady { get; private set; }
        

        public ChunkManager(
            ChunkGenerationScheduler scheduler,
            ChunkRepository repository,
            LandscapeApplier applier,
            ILandscapeFactory factory,
            IChunkNeighborConnector neighborConnector)
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
        
        public void Tick()
        {
            _scheduler.Tick(_applyAction, _completedAction);
        }
        
        public void Dispose()
        {
            _scheduler.CompleteAll();

            _repository.Dispose();
        }

        public void QueueLoad(
            ChunkCoordinate coordinate)
        {
            if(_repository.Contains(coordinate))
                return;

            if(!_loading.Add(coordinate))
                return;

            _scheduler.Enqueue( new ChunkGenerationRequest(coordinate, 257));
        }
        
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