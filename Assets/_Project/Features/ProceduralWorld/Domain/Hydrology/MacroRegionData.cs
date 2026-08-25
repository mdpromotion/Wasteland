using Unity.Collections;
using Unity.Mathematics;

namespace _Project.Features.ProceduralWorld.Domain.Hydrology
{
    /// <summary>
    /// Native data produced and consumed by macro-scale hydrology generation
    /// for a single macro region.
    /// </summary>
    /// <remarks>
    /// The data includes padded terrain and hydrology fields so generation can
    /// account for neighboring areas before extracting chunk-local results.
    /// </remarks>
    public sealed class MacroRegionData
    {
        /// <summary>
        /// Logical coordinate of this macro region.
        /// </summary>
        public MacroRegionCoordinate Coordinate { get; }
        
        /// <summary>
        /// Size of the region including padding, measured in cells.
        /// </summary>
        public int PaddedSize { get; }
        
        /// <summary>
        /// World-space size of a single macro-grid cell.
        /// </summary>
        public float CellSize { get; }
        
        /// <summary>
        /// Absolute world-space origin of the macro region.
        /// </summary>
        public double2 WorldOrigin { get; }

        public NativeArray<float> Heights;
        public NativeArray<sbyte> FlowDirection;
        public NativeArray<float> Accumulation;
        public NativeArray<float> WaterLevels;
        public NativeArray<float> RiverStrengthRaw;
        
        public NativeArray<float> RiverStrengthTight;
        
        public NativeArray<float> RiverStrengthSmoothed;

        private bool _disposed;

        public MacroRegionData(
            MacroRegionCoordinate coordinate,
            int paddedSize,
            float cellSize,
            double2 worldOrigin)
        {
            Coordinate = coordinate;
            PaddedSize = paddedSize;
            CellSize = cellSize;
            WorldOrigin = worldOrigin;

            int count = paddedSize * paddedSize;
            Heights = new NativeArray<float>(count, Allocator.Persistent);
            FlowDirection = new NativeArray<sbyte>(count, Allocator.Persistent);
            Accumulation = new NativeArray<float>(count, Allocator.Persistent);
            WaterLevels = new NativeArray<float>(count, Allocator.Persistent);
            RiverStrengthRaw = new NativeArray<float>(count, Allocator.Persistent);
            RiverStrengthTight = new NativeArray<float>(count, Allocator.Persistent);
            RiverStrengthSmoothed = new NativeArray<float>(count, Allocator.Persistent);
        }

        /// <summary>
        /// Releases all Native containers owned by this macro-region data.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (Heights.IsCreated) Heights.Dispose();
            if (FlowDirection.IsCreated) FlowDirection.Dispose();
            if (Accumulation.IsCreated) Accumulation.Dispose();
            if (WaterLevels.IsCreated) WaterLevels.Dispose();
            if (RiverStrengthRaw.IsCreated) RiverStrengthRaw.Dispose();
            if (RiverStrengthTight.IsCreated) RiverStrengthTight.Dispose();
            if (RiverStrengthSmoothed.IsCreated) RiverStrengthSmoothed.Dispose();
        }
    }
}