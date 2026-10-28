using UnityEngine;

namespace _Project.Features.Items.Infrastructure
{
    [CreateAssetMenu(menuName = "Project/Items/Drop Settings", fileName = "ItemDropSettings")]
    public sealed class ItemDropSettings : ScriptableObject
    {
        [SerializeField] private Vector3 spawnLocalOffset = new Vector3(0f, -0.1f, 1.25f);
        [SerializeField, Min(0f)] private float forwardSpeed = 3f;
        [SerializeField, Min(0f)] private float upwardSpeed = 1.2f;
        [SerializeField, Min(0.1f)] private float pickupRange = 2.5f;
        [SerializeField, Min(1)] private int maxVisibleBillboards = 6;

        public Vector3 SpawnLocalOffset => spawnLocalOffset;
        public float ForwardSpeed => forwardSpeed;
        public float UpwardSpeed => upwardSpeed;
        public float PickupRange => pickupRange;
        public int MaxVisibleBillboards => maxVisibleBillboards;
    }
}
