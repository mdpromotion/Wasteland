using _Project.Features.ProceduralWorld.Domain.Landscape;
using Unity.Collections;
using UnityEngine;

namespace _Project.Features.ProceduralWorld.Infrastructure
{
    /// <summary>
    /// Applies generated landscape data to a Unity Terrain representation.
    /// </summary>
    public interface ITerrainWriter
    {
        /// <summary>
        /// Copies the generated heightmap into the specified Unity Terrain.
        /// </summary>
        /// <param name="terrain">Terrain receiving the generated heightmap.</param>
        /// <param name="data">Generated landscape data for the terrain's chunk.</param>
        void Write(Terrain terrain, LandscapeData data);
    }
    
    /// <summary>
    /// Writes generated native landscape height data into a Unity Terrain heightmap.
    /// </summary>
    /// <remarks>
    /// The writer converts the flattened NativeArray representation used by the generation
    /// pipeline into the two-dimensional managed array expected by Unity Terrain.
    /// The conversion buffer is reused between writes to avoid repeated allocations.
    /// </remarks>
    public class UnityTerrainWriter : ITerrainWriter
    {
        private float[,] _buffer;
        
        public void Write(Terrain terrain, LandscapeData data)
        {
            EnsureBuffer(data.Resolution);
            
            FillBuffer(data);
            
            terrain.terrainData.SetHeightsDelayLOD(0, 0, _buffer);
        }



        private void EnsureBuffer(int resolution)
        {
            if (_buffer != null && _buffer.GetLength(0) == resolution)
                return;
            
            _buffer = new float[resolution, resolution];
        }



        private void FillBuffer(LandscapeData data)
        {
            int resolution = data.Resolution;

            NativeArray<float> heights = data.Heights;
            
            for (int y = 0; y < resolution; y++)
            {
                int row = y * resolution;

                for (int x = 0; x < resolution; x++)
                {
                    _buffer[y, x] = heights[row + x];
                }
            }
        }
    }
}