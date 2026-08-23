using System;
using System.Collections.Generic;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Infrastructure.Chunks;

namespace _Project.Features.ProceduralWorld.Application.Chunks
{
    public interface IChunkLookup
    {
        bool Contains(ChunkCoordinate coordinate);
        bool TryGet(ChunkCoordinate coordinate, out ChunkInstance chunk);
        ChunkInstance Get(ChunkCoordinate coordinate);
        IEnumerable<ChunkInstance> All { get; }
    }
    
    public class ChunkRepository : IChunkLookup, IDisposable
    {
        private readonly Dictionary<ChunkCoordinate, ChunkInstance> _chunks = new();
        
        public IEnumerable<ChunkInstance> All => _chunks.Values;

        public bool Contains(ChunkCoordinate coordinate)
        {
            return _chunks.ContainsKey(coordinate);
        }


        public bool TryGet(ChunkCoordinate coordinate, out ChunkInstance chunk)
            => _chunks.TryGetValue(coordinate, out chunk);


        public ChunkInstance Get(ChunkCoordinate coordinate)
            => _chunks.GetValueOrDefault(coordinate);


        public void Add(ChunkInstance chunk)
        {
            _chunks.Add(chunk.Coordinate, chunk);
        }


        public void Remove(ChunkCoordinate coordinate)
        {
            _chunks.Remove(coordinate);
        }


        public void Dispose()
        {
            foreach (ChunkInstance chunk in _chunks.Values)
            {
                if (chunk.Landscape != null)
                {
                    chunk.Landscape.Dispose();
                }

                if (chunk.Hydrology != null)
                {
                    chunk.Hydrology.Dispose();
                }
            }


            _chunks.Clear();
        }
    }
}