using Unity.Collections;

namespace _Project.Features.ProceduralWorld.Infrastructure.Vegetation
{
    public struct VegetationGenerationParams
    {
        public float Coverage;
        public float Density;
        
        public float EdgeSmoothing;
        
        public float MinScale;
        public float MaxScale;
        
        public float MinSlopeAngle;
        public float MaxSlopeAngle;
        
        public float PatchNoiseFrequency;
        public int PatchNoiseOctaves;

        public int Priority;

        public bool IsBreakable;

        public float OccupancyRadius;
    }
}