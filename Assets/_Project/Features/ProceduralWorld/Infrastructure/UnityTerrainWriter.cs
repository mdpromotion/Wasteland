using _Project.Features.ProceduralWorld.Domain.Landscape;
using Unity.Collections;
using UnityEngine;

namespace _Project.Features.ProceduralWorld.Infrastructure
{
    public interface ITerrainWriter
    {
        void Write(Terrain terrain, LandscapeData data);
    }
    
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