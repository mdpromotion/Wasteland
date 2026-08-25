namespace _Project.Features.ProceduralWorld.Domain.Hydrology
{
    /// <summary>
    /// Describes the water state sampled at a world-space location.
    /// </summary>
    public readonly struct WaterSample
    {
        /// <summary>
        /// Water-presence mask at the sampled location.
        /// </summary>
        public readonly float Mask;

        /// <summary>
        /// Absolute world-space height of the water surface.
        /// </summary>
        public readonly float WorldSurfaceHeight;

        public WaterSample(float mask, float worldSurfaceHeight)
        {
            Mask = mask;
            WorldSurfaceHeight = worldSurfaceHeight;
        }
        
        public bool IsSubmerged(float worldY, float maskThreshold)
        {
            return Mask >= maskThreshold && worldY <= WorldSurfaceHeight;
        }
    }
}