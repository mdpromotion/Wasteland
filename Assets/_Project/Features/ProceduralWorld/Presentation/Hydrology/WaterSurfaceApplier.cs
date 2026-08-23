using System;
using System.Collections.Generic;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Domain.Hydrology;
using _Project.Features.ProceduralWorld.Infrastructure.Hydrology;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Features.ProceduralWorld.Presentation.Hydrology
{
    public interface IWaterSurfaceApplier
    {
        void Apply(ChunkGenerationState state, Terrain terrain);
    }
    
    public sealed class WaterSurfaceApplier : IWaterSurfaceApplier, System.IDisposable
    {
        private static readonly int MaskHeightTexId = Shader.PropertyToID("_MaskHeightTex");
        private const string WaterChildName = "Water";

        private readonly ChunkGrid _grid;
        private readonly float _heightScale;
        private readonly Material _sharedMaterial;
        private readonly int _meshStride;

        private readonly Dictionary<Terrain, WaterHandle> _handles = new();

        public WaterSurfaceApplier(
            ChunkGrid grid,
            float heightScale,
            Material sharedMaterial,
            int meshStride = 1)
        {
            _grid = grid;
            _heightScale = heightScale;
            _sharedMaterial = sharedMaterial;
            _meshStride = meshStride;
        }

        public void Apply(ChunkGenerationState state, Terrain terrain)
        {
            WaterHandle handle = GetOrCreateHandle(terrain);

            if (handle == null)
                return;

            bool hasWater =
                state.WaterBounds.IsCreated &&
                state.WaterBounds.Length > 0 &&
                state.WaterBounds[0] == 1;

            handle.Root.gameObject.SetActive(hasWater);

            if (hasWater)
                UpdateWaterSurface(handle, state);
        }

        private WaterHandle GetOrCreateHandle(Terrain terrain)
        {
            if (_handles.TryGetValue(terrain, out WaterHandle handle))
                return handle;

            Transform water = terrain.transform.Find(WaterChildName);
            if (!water)
            {
                Debug.LogError(
                    $"WaterSurfaceApplier: '{terrain.name}' has no child named '{WaterChildName}'.");
                return null;
            }

            MeshFilter filter = water.GetComponent<MeshFilter>();
            if (!filter)
            {
                Debug.LogError($"WaterSurfaceApplier: '{WaterChildName}' has no MeshFilter.");
                return null;
            }

            Renderer renderer = water.GetComponent<Renderer>();
            if (!renderer)
            {
                Debug.LogError($"WaterSurfaceApplier: '{WaterChildName}' has no Renderer.");
                return null;
            }

            Mesh mesh = new Mesh { name = "WaterSurface" };
            filter.sharedMesh = mesh;

            if (!renderer.sharedMaterial)
                renderer.sharedMaterial = _sharedMaterial;

            handle = new WaterHandle(water, filter, renderer, mesh);
            _handles.Add(terrain, handle);

            return handle;
        }

        private void UpdateWaterSurface(WaterHandle handle, ChunkGenerationState state)
        {
            WaterMeshBuilder.Build(
                handle.Mesh,
                state.Hydrology.WaterSurfaceHeight,
                state.Context.Resolution,
                _grid.ChunkSizeX,
                _grid.ChunkSizeZ,
                _heightScale,
                _meshStride);

            int resolution = state.Context.Resolution;

            if (!handle.MaskTexture ||
                handle.MaskTexture.width != resolution ||
                handle.MaskTexture.height != resolution)
            {
                if (handle.MaskTexture)
                    Object.Destroy(handle.MaskTexture);

                handle.MaskTexture = new Texture2D(
                    resolution,
                    resolution,
                    TextureFormat.RGBA32,
                    mipChain: false,
                    linear: true)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                };
            }

            handle.MaskTexture.SetPixelData(state.WaterMaskPixels, 0);
            handle.MaskTexture.Apply(updateMipmaps: false, makeNoLongerReadable: false);

            handle.Renderer.GetPropertyBlock(handle.PropertyBlock);
            handle.PropertyBlock.SetTexture(MaskHeightTexId, handle.MaskTexture);
            handle.Renderer.SetPropertyBlock(handle.PropertyBlock);
        }

        public void Dispose()
        {
            foreach (WaterHandle handle in _handles.Values)
            {
                if (handle.MaskTexture)
                    Object.Destroy(handle.MaskTexture);

                if (handle.Mesh)
                    Object.Destroy(handle.Mesh);
            }

            _handles.Clear();
        }
    }
}