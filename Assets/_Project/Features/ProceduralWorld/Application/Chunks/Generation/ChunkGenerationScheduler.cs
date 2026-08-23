using System;
using System.Collections.Generic;
using _Project.Features.Core.Infrastructure;
using _Project.Features.ProceduralWorld.Application.Interfaces;
using _Project.Features.ProceduralWorld.Domain;
using _Project.Features.ProceduralWorld.Domain.Chunks;

namespace _Project.Features.ProceduralWorld.Application.Chunks.Generation
{
    public class ChunkGenerationScheduler
    {
        private readonly IChunkGenerator _pipeline;

        private readonly LinkedList<ChunkGenerationRequest> _queue = new();

        private readonly Dictionary<
            ChunkCoordinate,
            LinkedListNode<ChunkGenerationRequest>> _queued = new();

        private readonly List<GenerationTask> _running = new();
        private readonly ChunkCoordinateDistanceComparer _comparer = new();
        private readonly Comparison<GenerationTask> _comparison;

        private bool _needsSort;

        private const int MaxJobs = 10;
        
        private readonly IFrameBudget _frameBudget;


        public ChunkGenerationScheduler(
            IChunkGenerator pipeline,
            IFrameBudget frameBudget)
        {
            _pipeline = pipeline;
            _frameBudget = frameBudget;


            _comparison = (a, b) => _comparer.Compare(a.State.Context.Coordinate, b.State.Context.Coordinate);
        }



        public void Enqueue(ChunkGenerationRequest request)
        {
            if (_queued.ContainsKey(request.Coordinate))
                return;
            
            LinkedListNode<ChunkGenerationRequest> node = _queue.AddLast(request);
            
            _queued.Add(request.Coordinate, node);
        }



        public void Tick(Action<ChunkGenerationResult> apply, Action<ChunkCoordinate> completed)
        {
            Schedule();
            
            Complete(apply, completed);
        }



        private void Schedule()
        {
            while (_running.Count < MaxJobs && _queue.First != null)
            {
                LinkedListNode<ChunkGenerationRequest> node = _queue.First;
                
                _queue.RemoveFirst();
                _queued.Remove(node.Value.Coordinate);
                
                GenerationTask task = _pipeline.Schedule(node.Value);
                
                _running.Add(task);
                
                _needsSort = true;
            }
            
            if (_needsSort)
            {
                _running.Sort(_comparison);
                _needsSort = false;
            }
        }



        private void Complete(Action<ChunkGenerationResult> apply, Action<ChunkCoordinate> completed)
        {
            for (int i = 0; i < _running.Count;)
            {
                GenerationTask task = _running[i];

                if (!task.Handle.IsCompleted)
                {
                    i++;
                    continue;
                }
                
                if (!_frameBudget.TryBeginOperation( out IFrameBudgetOperation operation))
                    break;

                using (operation)
                {
                    task.Handle.Complete();

                    ChunkCoordinate coordinate = task.State.Context.Coordinate;

                    if (task.Cancelled)
                    {
                        task.State.DisposeAll();

                        completed(coordinate);

                        RemoveTask(i);

                        continue;
                    }

                    ChunkGenerationResult result = new ChunkGenerationResult(task.State);

                    apply(result);
                    
                    completed(coordinate);

                    RemoveTask(i);
                }
            }
        }



        private void RemoveTask(int index)
        {
            int last = _running.Count - 1;
            
            _running[index] = _running[last];
            
            _running.RemoveAt(last);
        }



        public void Cancel(ChunkCoordinate coordinate)
        {
            if (_queued.TryGetValue(coordinate, out LinkedListNode<ChunkGenerationRequest> node))
            {
                _queue.Remove(node);
                
                _queued.Remove(coordinate);
                
                return;
            }

            foreach (GenerationTask task in _running)
            {
                if(task.State.Context.Coordinate.Equals(coordinate))
                {
                    task.Cancelled = true;
                    return;
                }
            }
        }



        public void CompleteAll()
        {
            foreach(GenerationTask task in _running)
            {
                task.Handle.Complete();
                task.State.DisposeAll();
            }


            _running.Clear();
            _queue.Clear();
            _queued.Clear();
        }
    }
}