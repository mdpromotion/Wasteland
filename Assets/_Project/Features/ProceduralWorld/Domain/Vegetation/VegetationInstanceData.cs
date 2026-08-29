using Unity.Mathematics;

namespace _Project.Features.ProceduralWorld.Domain.Vegetation
{
    /// <summary>
    /// Transform and identity data for a single generated vegetation instance.
    /// </summary>
    public struct VegetationInstanceData
    {
        /// <summary>
        /// Local or chunk-relative position of the instance.
        /// </summary>
        public float3 Position;

        /// <summary>
        /// Rotation around the world/local Y axis, in degrees or radians as defined by the generator.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// Uniform scale applied to the instance.
        /// </summary>
        public float Scale;

        /// <summary>
        /// Can be broken by the player.
        /// </summary>
        public bool IsBreakable;

        /// <summary>
        /// Stable deterministic identifier derived from the global cell.
        /// Species is part of the full identity because instances are grouped by species layer.
        /// </summary>
        public ulong Id;
    }
}