using UnityEngine;

namespace _Project.Features.ProceduralWorld.Domain.World
{
    /// <summary>
    /// Authoring-time configuration for procedural world generation.
    /// </summary>
    /// <remarks>
    /// This ScriptableObject stores generation parameters that are copied into the
    /// runtime <see cref="WorldSettings"/> instance.
    /// </remarks>
    [CreateAssetMenu(menuName="Procedural World/World Settings")]
    public class WorldSettingsConfig : ScriptableObject
    {
        public int Octaves;
        public float Scale;
        public float Persistence;
        public float Lacunarity;
        public float RedistributionPower;
    }
}
