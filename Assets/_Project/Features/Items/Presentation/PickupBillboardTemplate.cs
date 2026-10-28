using UnityEngine;
using UnityEngine.UI;

namespace _Project.Features.Items.Presentation
{
    /// <summary>
    /// Serialization handles for the PickupBillboard prefab. The pool instantiates
    /// the prefab and refreshes content through these references instead of
    /// assembling the hierarchy in code.
    /// </summary>
    public sealed class PickupBillboardTemplate : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image background;
        [SerializeField] private Outline outline;
        [SerializeField] private Image iconBackground;
        [SerializeField] private Image icon;
        [SerializeField] private Text title;
        [SerializeField] private Text prompt;

        public Canvas Canvas => canvas;
        public Image Background => background;
        public Outline Outline => outline;
        public Image IconBackground => iconBackground;
        public Image Icon => icon;
        public Text Title => title;
        public Text Prompt => prompt;
    }
}
