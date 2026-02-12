using UnityEngine;
using GTAFramework.Vehicle.Data;
using GTAFramework.Vehicle.Components.Wheels;
using GTAFramework.Vehicle.Interfaces;

namespace GTAFramework.Vehicle.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour, IVehicle
    {
        [Header("References")]
        [SerializeField] private VehicleData _data;
        [SerializeField] private WheelController[] _wheels;

        [Header("Seats")]
        [SerializeField] private Transform _driverSeat;

        private Rigidbody _rb;
        private IDriver _currentDriver;

        // Propiedades públicas
        public VehicleData Data => _data;
        public Rigidbody Rigidbody => _rb;
        public WheelController[] Wheels => _wheels;
        public IDriver CurrentDriver => _currentDriver;

        // IVehicle implementation
        public bool IsOccupied => _currentDriver != null;
        public Transform Transform => transform;

        public void Enter(IDriver driver)
        {
            if (IsOccupied) return;

            _currentDriver = driver;
            driver.OnVehicleEnter(this);

            if (_driverSeat != null)
            {
                driver.Transform.position = _driverSeat.position;
                driver.Transform.rotation = _driverSeat.rotation;
                driver.Transform.SetParent(_driverSeat);
            }

            Debug.Log($"[VehicleController] {driver.Transform.name} entered vehicle {name}");
        }

        public void Exit()
        {
            if (!IsOccupied) return;

            Vector3 exitPosition = transform.position + transform.right * 2f;

            _currentDriver.Transform.SetParent(null);
            _currentDriver.Transform.position = exitPosition;
            _currentDriver.Transform.rotation = Quaternion.identity;

            _currentDriver.OnVehicleExit(this);
            Debug.Log($"[VehicleController] Driver exited vehicle {name}");

            _currentDriver = null;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            InitializeVehicle();
        }

        private void InitializeVehicle()
        {
            if (_data == null) return;

            _rb.mass = _data.mass;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            foreach (var wheel in _wheels)
            {
                wheel.Configure(_data);
            }
        }
    }
}