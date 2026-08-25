using System;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using Unity.Collections;

namespace _Project.Features.ProceduralWorld.Domain.Hydrology
{
    /// <summary>
    /// Native hydrology data generated for a single chunk.
    /// </summary>
    /// <remarks>
    /// The datasets use the chunk's generation resolution and are allocated as persistent
    /// Native containers. The instance owns these allocations and must be disposed when
    /// the hydrology data is no longer needed.
    /// </remarks>
    public sealed class HydrologyData
    {
        /// <summary>
        /// Logical chunk coordinate this data belongs to.
        /// </summary>
        public ChunkCoordinate Coordinate { get; }
        
        /// <summary>
        /// Resolution shared by all per-sample hydrology datasets.
        /// </summary>
        public int Resolution { get; }

        /// <summary>
        /// Accumulated water-flow amount at each sample.
        /// </summary>
        public NativeArray<float> Accumulation { get; }
        
        /// <summary>
        /// Normalized mask identifying river or water regions.
        /// </summary>
        public NativeArray<float> RiverMask { get; }
        
        /// <summary>
        /// Generated water-surface height at each sample.
        /// </summary>
        public NativeArray<float> WaterSurfaceHeight { get; }
        
        /// <summary>
        /// Height samples used to incorporate macro-scale terrain information.
        /// </summary>
        public NativeArray<float> MacroHeightSample { get; }
        
        /// <summary>
        /// Strength of the terrain embankment generated around water features.
        /// </summary>
        public NativeArray<float> EmbankmentStrength { get; }
        
        /// <summary>
        /// Flow direction encoded for each sample.
        /// </summary>
        public NativeArray<sbyte> FlowDirection;

        private readonly Action _onDispose;
        private bool _disposed;

        public HydrologyData(ChunkCoordinate coordinate, int resolution, Action onDispose)
        {
            Coordinate = coordinate;
            Resolution = resolution;
            _onDispose = onDispose;

            int count = resolution * resolution;

            Accumulation = new NativeArray<float>(count, Allocator.Persistent);
            RiverMask = new NativeArray<float>(count, Allocator.Persistent);
            WaterSurfaceHeight = new NativeArray<float>(count, Allocator.Persistent);
            MacroHeightSample = new NativeArray<float>(count, Allocator.Persistent);
            EmbankmentStrength = new NativeArray<float>(count, Allocator.Persistent);
            FlowDirection = new NativeArray<sbyte>(count, Allocator.Persistent);
        }

        /// <summary>
        /// Releases all Native containers owned by this hydrology dataset and invokes
        /// the optional disposal callback.
        /// </summary>
        /// <remarks>
        /// Disposal is idempotent; repeated calls have no effect.
        /// </remarks>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (Accumulation.IsCreated) Accumulation.Dispose();
            if (RiverMask.IsCreated) RiverMask.Dispose();
            if (WaterSurfaceHeight.IsCreated) WaterSurfaceHeight.Dispose();
            if (MacroHeightSample.IsCreated) MacroHeightSample.Dispose();
            if (EmbankmentStrength.IsCreated) EmbankmentStrength.Dispose();
            if (FlowDirection.IsCreated) FlowDirection.Dispose();

            _onDispose?.Invoke();
        }
    }
}