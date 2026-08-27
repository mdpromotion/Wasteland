using System.Collections.Generic;
using _Project.Features.ProceduralWorld.Domain.Chunks;

namespace _Project.Features.ProceduralWorld.Application.Persistence
{
    /// <summary>
    /// Tracks which chunks have unsaved mutations, decoupled from how/when they get flushed.
    /// </summary>
    public interface IDirtyChunkRegistry
    {
        void MarkDirty(ChunkCoordinate coord);
        void MarkClean(ChunkCoordinate coord);
        IReadOnlyCollection<ChunkCoordinate> DirtyChunks { get; }
        bool HasDirtyChunks { get; }
    }

    public sealed class DirtyChunkRegistry : IDirtyChunkRegistry
    {
        private readonly HashSet<ChunkCoordinate> _dirty = new();

        public void MarkDirty(ChunkCoordinate coord) => _dirty.Add(coord);
        public void MarkClean(ChunkCoordinate coord) => _dirty.Remove(coord);
        public IReadOnlyCollection<ChunkCoordinate> DirtyChunks => _dirty;
        public bool HasDirtyChunks => _dirty.Count > 0;
    }
}