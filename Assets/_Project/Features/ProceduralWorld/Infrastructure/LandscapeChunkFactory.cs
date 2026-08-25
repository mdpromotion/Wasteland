using System.Collections.Generic;
using _Project.Features.Graphics.Domain;
using _Project.Features.ProceduralWorld.Domain;
using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Infrastructure.Interfaces;
using _Project.Features.ProceduralWorld.Presentation;
using UnityEngine;

namespace _Project.Features.ProceduralWorld.Infrastructure
{
    /// <summary>
    /// Creates, pools, configures, and releases Unity Terrain instances used by generated chunks.
    /// </summary>
    /// <remarks>
    /// Terrain instances are reused through an internal pool to avoid repeated GameObject
    /// allocation during chunk streaming. Pool capacity follows the current graphics view distance.
    /// </remarks>
    public class LandscapeChunkFactory : ILandscapeFactory, System.IDisposable
    {
        private readonly Terrain _prefab;
        private readonly ChunkGrid _grid;
        private readonly GraphicsState _graphicsState;

        private readonly Dictionary<Terrain, ChunkHandle> _handles = new();

        private readonly Queue<Terrain> _pool;

        public LandscapeChunkFactory(
            Terrain prefab,
            ChunkGrid grid,
            GraphicsState graphicsState)
        {
            _prefab = prefab;
            _grid = grid;
            _graphicsState = graphicsState;
            
            int initialCapacity = GetMaxPoolCapacity(_graphicsState.ViewDistance);
            _pool = new Queue<Terrain>(initialCapacity);
            
            _graphicsState.GraphicsChanged += OnGraphicsChanged;
        }

        private void OnGraphicsChanged()
        {
            int maxPoolSize = GetMaxPoolCapacity(_graphicsState.ViewDistance);
            
            while (_pool.Count > maxPoolSize)
            {
                Terrain terrain = _pool.Dequeue();
                _handles.Remove(terrain);
                
                if (terrain != null)
                {
                    Object.Destroy(terrain.gameObject);
                }
            }
        }

        private int GetMaxPoolCapacity(int viewDistance)
        {
            return (viewDistance * 2 + 1) * 2;
        }

        public void Connect(Terrain terrain, Terrain left, Terrain top, Terrain right, Terrain bottom)
        {
            if (!terrain)
                return;

            terrain.SetNeighbors(left, top, right, bottom);
        }

        /// <summary>
        /// Creates or reuses a Terrain instance for the specified logical chunk coordinate.
        /// </summary>
        /// <remarks>
        /// Reused Terrain instances are reconfigured and repositioned rather than instantiated again.
        /// The returned Terrain is enabled for rendering and collision.
        /// </remarks>
        public Terrain Create(
            ChunkCoordinate coordinate,
            Transform parent)
        {
            Terrain terrain;
            ChunkHandle handle;

            if (_pool.Count > 0)
            {
                terrain = _pool.Dequeue();
                handle = _handles[terrain];

                Show(terrain, handle);
            }
            else
            {
                terrain = Object.Instantiate(_prefab, parent);

                terrain.drawHeightmap = true;
                terrain.drawTreesAndFoliage = true;
                terrain.drawInstanced = true;

                TerrainData data = CreateTerrainData();
                terrain.terrainData = data;

                TerrainCollider collider = terrain.GetComponent<TerrainCollider>();
                if (collider)
                    collider.terrainData = data;

                terrain.drawInstanced = true;
                terrain.heightmapPixelError = 20;

                TerrainChunkCoordinate marker = terrain.GetComponent<TerrainChunkCoordinate>();
                if (!marker)
                    marker = terrain.gameObject.AddComponent<TerrainChunkCoordinate>();

                handle = new ChunkHandle(
                    collider,
                    marker);

                _handles.Add(terrain, handle);
            }

            Vector2 worldOffset = _grid.ToWorldOffset(coordinate);

            terrain.transform.position = new Vector3(worldOffset.x, 0, worldOffset.y);

            handle.Marker.Initialize(coordinate);

            return terrain;
        }

        public void Show(Terrain terrain)
        {
            if (!_handles.TryGetValue(terrain, out ChunkHandle handle))
                return;

            Show(terrain, handle);
        }

        private void Show(Terrain terrain, ChunkHandle handle)
        {
            terrain.drawHeightmap = true;
            terrain.drawTreesAndFoliage = true;

            if (handle.Collider)
                handle.Collider.enabled = true;
        }

        /// <summary>
        /// Hides a Terrain, disables its collider, clears generated tree instances, and returns
        /// it to the pool or destroys it when the pool is full.
        /// </summary>
        public void Release(Terrain terrain)
        {
            terrain.drawHeightmap = false;
            terrain.drawTreesAndFoliage = false;

            if (_handles.TryGetValue(terrain, out ChunkHandle handle))
            {
                if (handle.Collider)
                    handle.Collider.enabled = false;
            }
            
            if (terrain.terrainData != null)
            {
                terrain.terrainData.treeInstances = System.Array.Empty<TreeInstance>();
            }

            int maxPoolSize = GetMaxPoolCapacity(_graphicsState.ViewDistance);

            if (_pool.Count >= maxPoolSize)
            {
                _handles.Remove(terrain);
                if (terrain != null)
                {
                    Object.Destroy(terrain.gameObject);
                }
            }
            else
            {
                _pool.Enqueue(terrain);
            }
        }

        private TerrainData CreateTerrainData()
        {
            TerrainData source = _prefab.terrainData;

            return new TerrainData
            {
                heightmapResolution = source.heightmapResolution,
                alphamapResolution = source.alphamapResolution,
                baseMapResolution = source.baseMapResolution,
                size = source.size,
                terrainLayers = source.terrainLayers
            };
        }
        
        /// <summary>
        /// Unsubscribes from graphics changes and clears the internal terrain pool and handle cache.
        /// </summary>
        /// <remarks>
        /// This method does not explicitly destroy pooled Terrain GameObjects.
        /// </remarks>
        public void Dispose()
        {
            if (_graphicsState != null)
            {
                _graphicsState.GraphicsChanged -= OnGraphicsChanged;
            }

            _handles.Clear();
            _pool.Clear();
        }
    }
}