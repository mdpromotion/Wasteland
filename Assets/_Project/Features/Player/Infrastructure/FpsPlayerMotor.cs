using _Project.Features.Player.Presentation;
using UnityEngine;

namespace _Project.Features.Player.Infrastructure
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public sealed class FpsPlayerMotor : MonoBehaviour, IFpsPlayerMotor
    {
        [Header("Ground Check")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundCheckRadius = 0.35f;
        [SerializeField] private float groundCheckOffset = 0.05f;

        private Rigidbody _rb;
        private Collider _collider;
        private bool _initialized;

        public Vector3 CurrentVelocity =>
            _rb.linearVelocity;

        public Vector3 CurrentPosition
        {
            get
            {
                EnsureInitialized();
                return _rb.position;
            }
        }
        
        public bool IsAlive => this;

        public void Freeze(bool state)
        {
            if (!_rb)
                return;

            if (state)
            {
                _rb.isKinematic = true;
                _rb.useGravity = false;
            }
            else
            {
                _rb.isKinematic = false;
                _rb.useGravity = true;
            }
        }

        
        private void Awake()
        {
            EnsureInitialized();
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        
        private void EnsureInitialized()
        {
            if (_initialized)
                return;

            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _initialized = true;
        }

        public void SetVelocity(Vector3 velocity)
        {
            EnsureInitialized();
            
            _rb.linearVelocity = velocity;
        }

        public void SetRotation(Quaternion rotation)
        {
            EnsureInitialized();
            
            _rb.MoveRotation(rotation);
        }

        public bool IsGroundedCheck()
        {
            return Physics.CheckSphere(
                GetGroundCheckPosition(),
                groundCheckRadius,
                groundMask,
                QueryTriggerInteraction.Ignore);
        }

        public bool TryGetSafeGroundPosition(out Vector3 position)
        {
            EnsureInitialized();
            
            Bounds bounds =
                _collider.bounds;

            Vector3 origin =
                new Vector3(
                    bounds.center.x,
                    bounds.min.y +
                    groundCheckOffset,
                    bounds.center.z);

            if (Physics.Raycast(
                    origin,
                    Vector3.down,
                    out RaycastHit hit,
                    Mathf.Infinity,
                    groundMask,
                    QueryTriggerInteraction.Ignore))
            {
                float bottomOffset =
                    _rb.position.y -
                    bounds.min.y;

                position =
                    new Vector3(
                        _rb.position.x,
                        hit.point.y +
                        bottomOffset,
                        _rb.position.z);

                return true;
            }

            position = default;

            return false;
        }

        public void TeleportToPosition(Vector3 position)
        {
            EnsureInitialized();

            _rb.position = position;
            _rb.linearVelocity = Vector3.zero;
        }
        
        public void ApplyOriginShift(Vector3 delta)
        {
            var interpolation = _rb.interpolation;
            _rb.interpolation = RigidbodyInterpolation.None;
            
            transform.position += delta;
            
            Physics.SyncTransforms();
            
            _rb.interpolation = interpolation;
        }

        private Vector3 GetGroundCheckPosition()
        {
            if (_collider)
            {
                Bounds bounds =
                    _collider.bounds;

                return new Vector3(
                    bounds.center.x,
                    bounds.min.y +
                    groundCheckOffset,
                    bounds.center.z);
            }

            return _rb.position +
                   Vector3.down *
                   groundCheckOffset;
        }
    }
}