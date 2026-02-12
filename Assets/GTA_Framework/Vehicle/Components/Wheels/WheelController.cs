using GTAFramework.Vehicle.Data;
using UnityEngine;

namespace GTAFramework.Vehicle.Components.Wheels
{
    public class WheelController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool _isPowered = true;
        [SerializeField] private bool _isSteerable = true;
        [SerializeField] private bool _isRear = false; // Para el handbrake

        [Header("Visuals")]
        [SerializeField] private Transform _wheelMesh;

        private WheelCollider _wheelCollider;

        public bool IsPowered => _isPowered;
        public bool IsSteerable => _isSteerable;
        public bool IsRear => _isRear;
        public bool IsGrounded => _wheelCollider != null && _wheelCollider.isGrounded;

        // ========== PROPIEDADES DE INPUT (Wrappers del WheelCollider) ==========

        /// <summary>
        /// Ángulo de dirección en grados.
        /// </summary>
        public float SteerAngle
        {
            get => _wheelCollider?.steerAngle ?? 0f;
            set
            {
                if (_wheelCollider != null)
                    _wheelCollider.steerAngle = value;
            }
        }

        /// <summary>
        /// Torque del motor en Nm.
        /// </summary>
        public float MotorTorque
        {
            get => _wheelCollider?.motorTorque ?? 0f;
            set
            {
                if (_wheelCollider != null)
                    _wheelCollider.motorTorque = value;
            }
        }

        /// <summary>
        /// Torque de freno en Nm.
        /// </summary>
        public float BrakeTorque
        {
            get => _wheelCollider?.brakeTorque ?? 0f;
            set
            {
                if (_wheelCollider != null)
                    _wheelCollider.brakeTorque = value;
            }
        }

        private void Awake()
        {
            _wheelCollider = GetComponent<WheelCollider>();
            if (_wheelCollider == null)
                _wheelCollider = gameObject.AddComponent<WheelCollider>();
        }

        private void Update()
        {
            UpdateWheelVisuals();
        }

        private void UpdateWheelVisuals()
        {
            if (_wheelMesh == null || _wheelCollider == null) return;

            _wheelCollider.GetWorldPose(out Vector3 position, out Quaternion rotation);
            _wheelMesh.position = position;
            _wheelMesh.rotation = rotation;
        }

        public void Configure(VehicleData data)
        {
            _wheelCollider.radius = data.wheelRadius;
            _wheelCollider.suspensionDistance = data.suspensionDistance;

            var spring = new JointSpring
            {
                spring = data.suspensionSpring,
                damper = data.suspensionDamper,
                targetPosition = 0.5f
            };
            _wheelCollider.suspensionSpring = spring;
        }
    }
}