using System;
using System.Collections.Generic;
using _Project.Features.Persistence.Infrastructure;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Persistence;

namespace _Project.Features.Persistence.Application
{
    public interface IChunkDeltaStore
    {
        ChunkDelta Load(ChunkCoordinate coord);
        void Save(ChunkCoordinate coord, ChunkDelta incrementalDelta);
        void Unload(ChunkCoordinate coord);
    }
    
    public sealed class ChunkDeltaStore : IChunkDeltaStore
    {
        private readonly IPalRegionReader _reader;
        private readonly IPalRegionWriter _writer;
        private readonly ChunkDeltaSerializer _serializer;
        
        private readonly Dictionary<ChunkCoordinate, ChunkDelta> _cache = new();

        public ChunkDeltaStore(IPalRegionReader reader, IPalRegionWriter writer, ChunkDeltaSerializer serializer)
        {
            _reader = reader;
            _writer = writer;
            _serializer = serializer;
        }

        public ChunkDelta Load(ChunkCoordinate coord)
        {
            if (_cache.TryGetValue(coord, out var cachedDelta))
            {
                return cachedDelta;
            }
            
            var (regionX, regionZ, slot) = RegionAddressing.ToSlot(coord);
            var result = _reader.ReadSlot(regionX, regionZ, slot);

            var delta = result.State switch
            {
                PalSlotState.Present => _serializer.Deserialize(result.Payload),
                PalSlotState.Missing => ChunkDelta.Empty,
                PalSlotState.Tombstoned => ChunkDelta.Empty,
                PalSlotState.Corrupted => ChunkDelta.Empty, // TODO: сигнал наверх, не молчать
                _ => ChunkDelta.Empty
            };
            
            _cache[coord] = delta;
            
            return delta;
        }

        public void Save(ChunkCoordinate coord, ChunkDelta incrementalDelta)
        {
            var existingDelta = Load(coord);
            
            var finalDelta = existingDelta.Merge(incrementalDelta);
            
            _cache[coord] = finalDelta;
            
            var (regionX, regionZ, slot) = RegionAddressing.ToSlot(coord);

            if (finalDelta.IsEmpty)
            {
                _writer.DeleteSlot(regionX, regionZ, slot);
                return;
            }

            var payload = _serializer.Serialize(finalDelta);
            _writer.WriteSlot(regionX, regionZ, slot, payload);
        }
        
        public void Unload(ChunkCoordinate coord)
        {
            _cache.Remove(coord);
        }
    }

    internal static class RegionAddressing
    {
        public static (int regionX, int regionZ, int slot) ToSlot(ChunkCoordinate c)
        {
            int regionX = FloorDiv(c.X, PalRegionFormat.RegionSize);
            int regionZ = FloorDiv(c.Y, PalRegionFormat.RegionSize);
            int localX = Mod(c.X, PalRegionFormat.RegionSize);
            int localZ = Mod(c.Y, PalRegionFormat.RegionSize);
            return (regionX, regionZ, localZ * PalRegionFormat.RegionSize + localX);
        }

        private static int FloorDiv(int a, int b) => (int)Math.Floor(a / (double)b);
        private static int Mod(int a, int b) => ((a % b) + b) % b;
    }
}