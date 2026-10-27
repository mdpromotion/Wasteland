using _Project.Features.Player.Domain;
using UnityEngine;

namespace _Project.Features.Player.Infrastructure
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RigidbodyPlayerState : MonoBehaviour, IPlayerReadOnly
    {
        [SerializeField] private Rigidbody _rb;

        public bool IsAvailable => this != null && _rb != null;

        public Vector3 Position => this == null
            ? Vector3.zero
            : _rb != null
                ? _rb.position
                : transform.position;

        public Vector3 Velocity => IsAvailable ? _rb.linearVelocity : Vector3.zero;

        public Vector3 Forward => this != null ? transform.forward : Vector3.forward;


        private void Awake()
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();
        }
        
    }
}
