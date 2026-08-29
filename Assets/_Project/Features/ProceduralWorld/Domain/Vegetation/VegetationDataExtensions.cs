namespace _Project.Features.ProceduralWorld.Domain.Vegetation
{
    public static class VegetationDataExtensions
    {
        /// <summary>
        /// Removes a single vegetation instance identified purely by species + Id.
        /// </summary>
        public static bool TryRemoveInstance(this VegetationData vegetation, VegetationSpeciesType species, ulong id)
        {
            foreach (var layer in vegetation.Layers)
            {
                if (layer.Species != species || !layer.Instances.IsCreated)
                    continue;

                var instances = layer.Instances;

                for (int i = 0; i < instances.Length; i++)
                {
                    if (instances[i].Id != id)
                        continue;

                    instances.RemoveAtSwapBack(i);
                    return true;
                }
            }

            return false;
        }
    }
}