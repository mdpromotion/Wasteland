using Unity.Collections;
using UnityEngine;
using _Project.Features.ProceduralWorld.Domain.Hydrology;
using _Project.Features.ProceduralWorld.Domain.Landscape;
using _Project.Features.ProceduralWorld.Domain.Vegetation;

namespace _Project.Features.ProceduralWorld.Domain.Chunks
{
    /// <summary>
    /// Mutable state accumulated by the generation stages of a single chunk.
    /// </summary>
    /// <remarks>
    /// The same state instance is passed through the generation pipeline, allowing
    /// later stages to consume data produced by earlier stages. The state also owns
    /// the Native containers created for the generation result until ownership is
    /// transferred to <see cref="ChunkGenerationResult"/> or the state is disposed.
    /// </remarks>
    public sealed class ChunkGenerationState
    {
        /// <summary>
        /// Immutable parameters identifying the chunk and its generation resolution.
        /// </summary>
        public ChunkGenerationContext Context { get; }

        /// <summary>
        /// Generated terrain height data for the chunk.
        /// </summary>
        public LandscapeData Landscape { get; set; }

        /// <summary>
        /// Generated hydrology data for the chunk.
        /// </summary>
        public HydrologyData Hydrology { get; set; }

        /// <summary>
        /// Generated vegetation data for the chunk.
        /// </summary>
        public VegetationData Vegetation { get; set; }

        /// <summary>
        /// Pixel data used to represent the generated water mask.
        /// </summary>
        public NativeArray<Color32> WaterMaskPixels { get; set; }

        /// <summary>
        /// Bounds of the generated water area.
        /// </summary>
        public NativeArray<int> WaterBounds { get; set; }

        /// <summary>
        /// Average generated water-surface height data.
        /// </summary>
        public NativeArray<float> WaterAverageHeight { get; set; }

        public ChunkGenerationState(ChunkGenerationContext context)
        {
            Context = context;
        }
        
        /// <summary>
        /// Disposes all generated data currently owned by this state.
        /// </summary>
        /// <remarks>
        /// Intended for generation paths where the state will not be transferred to
        /// a successfully completed chunk, such as cancellation or shutdown.
        /// </remarks>
        public void DisposeAll()
        {
            Landscape?.Dispose();
            Hydrology?.Dispose();
            Vegetation?.Dispose();

            if(WaterMaskPixels.IsCreated)
                WaterMaskPixels.Dispose();

            if(WaterBounds.IsCreated)
                WaterBounds.Dispose();

            if(WaterAverageHeight.IsCreated)
                WaterAverageHeight.Dispose();
        }
    }
}