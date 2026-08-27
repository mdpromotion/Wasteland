using Unity.Collections;

namespace _Project.Features.ProceduralWorld.Domain.Vegetation
{
    /// <summary>
    /// Native vegetation instances belonging to a single species category.
    /// </summary>
    public sealed class VegetationLayerData
    {
        public VegetationSpeciesType Species { get; }
        public NativeList<VegetationInstanceData> Instances { get; }

        public VegetationLayerData(VegetationSpeciesType species, NativeList<VegetationInstanceData> instances)
        {
            Species = species;
            Instances = instances;
        }

        public void Dispose()
        {
            if (Instances.IsCreated)
                Instances.Dispose();
        }
    }
}