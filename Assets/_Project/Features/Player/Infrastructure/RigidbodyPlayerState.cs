using _Project.Features.Player.Domain;
using UnityEngine;

namespace _Project.Features.Player.Infrastructure
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RigidbodyPlayerState : MonoBehaviour, IPlayerReadOnly
    {
        [SerializeField] private Rigidbody _rb;

        public Vector3 Position => _rb.position;
        public Vector3 Velocity => _rb.linearVelocity;
        
        public Vector3 Forward => _rb.transform.forward;


        private void Awake()
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();
        }
        
    }
}