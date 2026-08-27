using System;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Persistence;

namespace _Project.Features.Core.Persistence.Regions
{
    public sealed class ChunkDeltaStore
    {
        private readonly IPalRegionReader _reader;
        private readonly IPalRegionWriter _writer;
        private readonly ChunkDeltaSerializer _serializer;

        public ChunkDeltaStore(IPalRegionReader reader, IPalRegionWriter writer, ChunkDeltaSerializer serializer)
        {
            _reader = reader;
            _writer = writer;
            _serializer = serializer;
        }

        public ChunkDelta Load(ChunkCoordinate coord)
        {
            var (regionX, regionZ, slot) = RegionAddressing.ToSlot(coord);
            var result = _reader.ReadSlot(regionX, regionZ, slot);

            return result.State switch
            {
                PalSlotState.Present => _serializer.Deserialize(result.Payload),
                PalSlotState.Missing => ChunkDelta.Empty,
                PalSlotState.Tombstoned => ChunkDelta.Empty,
                PalSlotState.Corrupted => ChunkDelta.Empty, // TODO: сигнал наверх, не молчать
                _ => ChunkDelta.Empty
            };
        }

        public void Save(ChunkCoordinate coord, ChunkDelta delta)
        {
            var (regionX, regionZ, slot) = RegionAddressing.ToSlot(coord);

            if (delta.IsEmpty)
            {
                _writer.DeleteSlot(regionX, regionZ, slot);
                return;
            }

            var payload = _serializer.Serialize(delta);
            _writer.WriteSlot(regionX, regionZ, slot, payload);
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