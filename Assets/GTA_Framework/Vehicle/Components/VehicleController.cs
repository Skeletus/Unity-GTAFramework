using UnityEngine;
using GTAFramework.Vehicle.Data;
using GTAFramework.Vehicle.Components.Wheels;

namespace GTAFramework.Vehicle.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private VehicleData _data;
        [SerializeField] private WheelController[] _wheels;
        
        private Rigidbody _rb;
        
        public VehicleData Data => _data;
        public Rigidbody Rigidbody => _rb;
        public WheelController[] Wheels => _wheels;
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            InitializeVehicle();
        }
        
        private void InitializeVehicle()
        {
            if (_data == null) return;
            
            // Configure Rigidbody
            _rb.mass = _data.mass;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            
            // Configure wheels
            foreach (var wheel in _wheels)
            {
                wheel.Configure(_data);
            }
        }
    }
}