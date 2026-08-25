using UnityEngine;

namespace _Project.Features.ProceduralWorld.Domain.World
{
    /// <summary>
    /// Configuration controlling when the procedural world origin is rebased.
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural World/World Rebase Settings")]
    public class WorldRebaseSettings : ScriptableObject
    {
        /// <summary>
        /// Maximum allowed distance, measured in chunks, between the current streaming
        /// center and the grid origin before a world rebase is performed.
        /// </summary>
        [Min(1)]
        public int ThresholdChunks = 64;
    }
}