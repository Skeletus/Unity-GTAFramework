using UnityEngine;
using GTAFramework.Vehicle.Data;
using GTAFramework.Vehicle.Components.Wheels;
using GTAFramework.Vehicle.Interfaces;
using GTAFramework.Vehicle.States;
using static UnityEngine.UI.Image;

namespace GTAFramework.Vehicle.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour, IVehicle
    {
        [Header("References")]
        [SerializeField] private VehicleData _data;
        [SerializeField] private WheelController[] _wheels;
        [SerializeField] private MeshFilter _bodyMesh;

        [Header("Seats")]
        [SerializeField] private Transform _driverSeat;

        [Header("Ground Detection")]
        [SerializeField] private LayerMask _groundMask = -1;
        [SerializeField] private float _groundCheckDistance = 0.5f;

        [Header("Stability")]
        [SerializeField] private Vector3 _centerOfMass = new Vector3(0f, -0.5f, 0f);

        private Rigidbody _rb;
        private VehiclePhysics _physics;
        private IDriver _currentDriver;

        // ========== STATE MACHINE ==========
        private VehicleState _currentState;
        public ParkedState ParkedState { get; private set; }
        public DrivingState DrivingState { get; private set; }
        public Vehicle_AirborneState AirborneState { get; private set; }

        // Campo privado
        private VehicleDamage _damage;

        [Header("Debug")]
        [SerializeField] private bool _showStateDebug = true;
        [SerializeField] private float _currentSpeedDebug;
        private string _currentStateName = "";

        // Propiedades públicas
        public VehicleData Data => _data;
        public Rigidbody Rigidbody => _rb;
        public WheelController[] Wheels => _wheels;
        public IDriver CurrentDriver => _currentDriver;
        public VehiclePhysics Physics => _physics;
        public VehicleState CurrentState => _currentState;
        public DestroyedState DestroyedState { get; private set; }
        public string CurrentStateName => _currentStateName;
        // Propiedad pública
        public VehicleDamage Damage => _damage;
        public bool IsDestroyed => _damage?.IsDestroyed ?? false;


        // IVehicle implementation
        public bool IsOccupied => _currentDriver != null;
        public Transform Transform => transform;

        // Ground detection
        public bool IsGrounded { get; private set; }
        // Umbral mínimo de velocidad (0.1 m/s = casi detenido)
        private const float SPEED_THRESHOLD = 0.1f;

        public float CurrentSpeed
        {
            get
            {
                if (_rb == null) return 0f;

                float rawSpeed = _rb.linearVelocity.magnitude;

                // Si la velocidad es menor al umbral, considerar como detenido
                if (rawSpeed < SPEED_THRESHOLD) return 0f;

                // Redondear a 1 decimal
                return Mathf.Round(rawSpeed * 10f) / 10f;
            }
        }

        public void Enter(IDriver driver)
        {
            if (IsOccupied) return;

            // No permitir entrar si está destruido
            if (IsDestroyed)
            {
                Debug.Log($"[VehicleController] Cannot enter destroyed vehicle: {name}");
                return;
            }

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
        }

        private void Start()
        {
            InitializeVehicle();
            InitializeStateMachine();
        }

        private void Update()
        {
            _currentSpeedDebug = CurrentSpeed;
            UpdateGroundDetection();
            UpdateStateMachine();
        }

        private void FixedUpdate()
        {
            if (IsDestroyed)
            {
                // Mantener freno de mano activo
                if (_physics != null)
                {
                    _physics.Handbrake = true;
                }
                return;
            }

            // Solo procesar física si hay un conductor
            if (IsOccupied && _physics != null)
            {
                _physics.FixedUpdate();
            }
        }

        private void InitializeVehicle()
        {
            if (_data == null) return;

            _rb.mass = _data.mass;
            _rb.centerOfMass = _centerOfMass;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            _physics = new VehiclePhysics(this, _rb, _data);

            _damage = new VehicleDamage(this, _data, _bodyMesh);
            _damage.OnDestroy += HandleVehicleDestroyed;

            foreach (var wheel in _wheels)
            {
                wheel.Configure(_data);
            }
        }

        // ========== STATE MACHINE ==========

        private void InitializeStateMachine()
        {
            ParkedState = new ParkedState(this);
            DrivingState = new DrivingState(this);
            AirborneState = new Vehicle_AirborneState(this);
            DestroyedState = new DestroyedState(this);

            // Estado inicial: Parked
            _currentState = ParkedState;
            _currentStateName = _currentState.GetStateName();
            _currentState.Enter();

            if (_showStateDebug)
                Debug.Log($"[VehicleController] State Machine initialized. Initial state: {_currentStateName}");
        }

        private void UpdateStateMachine()
        {
            if (_currentState == null) return;

            _currentState.Update();

            VehicleState nextState = _currentState.CheckTransitions();

            if (nextState != null && nextState != _currentState)
            {
                TransitionToState(nextState);
            }
        }

        private void TransitionToState(VehicleState newState)
        {
            if (newState == null || newState == _currentState) return;

            string previousStateName = _currentStateName;

            _currentState.Exit();
            _currentState = newState;
            _currentStateName = _currentState.GetStateName();
            _currentState.Enter();

            if (_showStateDebug)
                Debug.Log($"[VehicleController] State transition: {previousStateName} → {_currentStateName}");
        }

        /// <summary>
        /// Fuerza una transición de estado (útil para sistemas externos).
        /// </summary>
        public void ForceState(VehicleState newState)
        {
            if (newState != null)
                TransitionToState(newState);
        }

        // Nuevo método para colisiones
        private void OnCollisionEnter(Collision collision)
        {
            // No procesar si ya está destruido
            if (IsDestroyed) return;

            // Solo procesar colisiones significativas
            float impactForce = collision.relativeVelocity.magnitude;
            if (impactForce > 5f) // Umbral mínimo de impacto
            {
                _damage?.HandleCollision(collision);
            }
        }

        // Nuevo método para evento de destrucción
        private void HandleVehicleDestroyed()
        {
            // Transicionar a DestroyedState
            ForceState(DestroyedState);
        }

        // ========== GROUND DETECTION ==========

        private void UpdateGroundDetection()
        {
            // Raycast desde el centro del vehículo hacia abajo
            IsGrounded = false;
            foreach (var wheel in _wheels)
            {
                if (wheel.IsGrounded)
                {
                    IsGrounded = true;
                    break;
                }
            }
        }
    }
}