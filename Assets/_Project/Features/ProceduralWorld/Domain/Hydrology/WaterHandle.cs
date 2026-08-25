using UnityEngine;

namespace _Project.Features.ProceduralWorld.Domain.Hydrology
{
    /// <summary>
    /// Holds the Unity objects associated with a generated water representation.
    /// </summary>
    /// <remarks>
    /// The handle groups the scene objects and per-renderer state required to update
    /// a water surface without repeatedly looking them up on the generated GameObject.
    /// </remarks>
    public sealed class WaterHandle
    {
        public readonly Transform Root;
        public readonly MeshFilter Filter;
        public readonly Renderer Renderer;
        public readonly Mesh Mesh;
        public readonly MaterialPropertyBlock PropertyBlock;

        public Texture2D MaskTexture;

        public WaterHandle(Transform root, MeshFilter filter, Renderer renderer, Mesh mesh)
        {
            Root = root;
            Filter = filter;
            Renderer = renderer;
            Mesh = mesh;
            PropertyBlock = new MaterialPropertyBlock();
        }
    }
}