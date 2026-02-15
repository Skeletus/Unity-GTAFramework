using UnityEngine;
using GTAFramework.Vehicle.Data;
using GTAFramework.Vehicle.Components.Wheels;
using GTAFramework.Vehicle.Interfaces;
using GTAFramework.Vehicle.States;
using System.Linq;
using GTAFramework.Vehicle.Components.VehicleCollision;
using GTAFramework.Vehicle.StateMachine;
using System;
using GTAFramework.Player.Components.States;
using GTAFramework.Vehicle.VehicleDriverManager;

namespace GTAFramework.Vehicle.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour, IVehicle, IVehicleContext
    {
        [Header("References")]
        [SerializeField] private VehicleData _data;
        [SerializeField] private WheelController[] _wheels;
        [SerializeField] private MeshFilter _bodyMesh;
        [SerializeField] private Transform _driverSeat;
        [SerializeField] private Vector3 _centerOfMass = new Vector3(0f, -0.5f, 0f);

        // Componentes inyectados
        private Rigidbody _rb;
        private IVehicleStateMachine _stateMachine;
        private IDriverManager _driverManager;
        private VehiclePhysics _physics;
        private IVehicleDamage _damage;
        private IVehicleCollisionHandler _collisionHandler;

        // Propiedades públicas (delegación)
        public bool IsOccupied => _driverManager?.IsOccupied ?? false;
        public bool IsGrounded => _wheels?.Any(w => w.IsGrounded) ?? false;
        public bool IsDestroyed => _damage?.IsDestroyed ?? false;
        public float CurrentSpeed => CalculateSpeed();

        public Transform Transform => transform;

        public IVehiclePhysics Physics => _physics;
        public Rigidbody Rigidbody => _rb;
        public WheelController[] Wheels => _wheels;
        public VehicleData Data => _data;

        public void Enter(IDriver driver) => _driverManager?.Enter(driver);
        public void Exit() => _driverManager?.Exit();


        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            InitializeComponents();
            _stateMachine.TransitionTo(VehicleStateNames.Parked);
        }

        private void Update()
        {
            _stateMachine?.Update();
        }

        private void FixedUpdate()
        {
            if (IsDestroyed) { _physics.Handbrake = true; return; }
            if (IsOccupied) _physics?.FixedUpdate();
        }

        private void InitializeComponents()
        {
            if (_data == null) return;

            // 1. Configurar Rigidbody
            _rb.mass = _data.mass;
            _rb.centerOfMass = _centerOfMass;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // 2. Crear componentes del vehículo
            _physics = new VehiclePhysics(this, _rb, _data, _wheels);

            _damage = new VehicleDamage(this, _data, _bodyMesh);
            _damage.OnDestroy += HandleVehicleDestroyed;

            // 3. Crear managers (composición)
            _driverManager = new DriverManager(this, _driverSeat, () => !IsDestroyed);
            _collisionHandler = new VehicleCollisionHandler(_damage, () => IsDestroyed);

            // 4. Crear máquina de estados
            _stateMachine = new VehicleStateMachine();
            _stateMachine.RegisterState(VehicleStateNames.Parked, new ParkedState(this));
            _stateMachine.RegisterState(VehicleStateNames.Driving, new DrivingState(this));
            _stateMachine.RegisterState(VehicleStateNames.Airborne, new VehicleAirborneState(this));
            _stateMachine.RegisterState(VehicleStateNames.Destroyed, new DestroyedState(this));

            // 5. Configurar ruedas
            foreach (var wheel in _wheels)
            {
                wheel.Configure(_data);
            }
        }

        private float CalculateSpeed()
        {
            if (_rb == null) return 0f;

            float rawSpeed = _rb.linearVelocity.magnitude;

            // Si la velocidad es menor al umbral, considerar como detenido
            if (rawSpeed < _data.SPEED_THRESHOLD) return 0f;

            // Redondear a 1 decimal
            return Mathf.Round(rawSpeed * 10f) / 10f;
        }

        private void OnCollisionEnter(Collision collision)
        => _collisionHandler?.HandleCollision(collision);

        private void HandleVehicleDestroyed()
            => _stateMachine?.TransitionTo(VehicleStateNames.Destroyed);
    }
}