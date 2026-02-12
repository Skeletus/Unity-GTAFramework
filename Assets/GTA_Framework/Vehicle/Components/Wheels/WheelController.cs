using GTAFramework.Vehicle.Data;
using UnityEngine;

namespace GTAFramework.Vehicle.Components.Wheels
{
    public class WheelController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool _isPowered = true;
        [SerializeField] private bool _isSteerable = true;

        [Header("Visuals")]
        [SerializeField] private Transform _wheelMesh;

        private WheelCollider _wheelCollider;

        public bool IsPowered => _isPowered;
        public bool IsSteerable => _isSteerable;

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