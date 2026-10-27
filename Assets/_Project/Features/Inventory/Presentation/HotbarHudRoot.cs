using UnityEngine;

namespace _Project.Features.Items.Presentation
{
    public sealed class HotbarHudRoot
    {
        public Transform Transform { get; }

        public HotbarHudRoot(Transform transform)
        {
            Transform = transform;
        }
    }
}
