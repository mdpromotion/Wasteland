using UnityEngine;

namespace _Project.Features.Items.Presentation
{
    /// <summary>
    /// Presentation-only configuration of how an item is shown in the player's
    /// hands. Item definitions reference one of these instead of carrying
    /// view-model data themselves, so gameplay config stays presentation-agnostic.
    /// </summary>
    /// <remarks>
    /// All fields are effectively optional: a null reference is treated by the
    /// presentation layer as "use the global defaults"
    /// (<see cref="HotbarHudSettings"/>), and a null view-model prefab falls
    /// back to the reused world prefab visual, then to the colored cube.
    /// </remarks>
    [CreateAssetMenu(menuName = "Project/Items/Held Item Config", fileName = "HeldItemConfig")]
    public sealed class HeldItemConfig : ScriptableObject
    {
        [Header("View Model")]
        [SerializeField, Tooltip("Optional view-model shown in the hands while equipped. Empty falls back to the item's world prefab, then to a colored cube.")]
        private GameObject visualPrefab;

        [Header("Held Pose")]
        [SerializeField, Tooltip("Local position of the view-model relative to the camera.")]
        private Vector3 heldPosition = new Vector3(0.42f, -0.34f, 0.72f);
        [SerializeField, Tooltip("Local rotation (Euler degrees) of the view-model in the hands.")]
        private Vector3 heldRotation = new Vector3(12f, -18f, 6f);
        [SerializeField, Tooltip("Uniform-free scale of the whole view-model; zero axes are treated as unset.")]
        private Vector3 heldScale = new Vector3(0.18f, 0.18f, 0.5f);

        public GameObject VisualPrefab => visualPrefab;
        public Vector3 HeldPosition => heldPosition;
        public Vector3 HeldRotation => heldRotation;
        public Vector3 HeldScale => heldScale;
    }
}
