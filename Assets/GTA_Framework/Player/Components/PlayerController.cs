using UnityEngine;
using GTAFramework.Player.Data;
using GTAFramework.GTA_Animation.Components;
using GTAFramework.Player.Components.States;
using GTAFramework.Vehicle.Interfaces;

namespace GTAFramework.Player.Components
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour, CharacterAnimationAgent.ICharacterAnimationSource, IDriver
    {
        [Header("References")]
        [SerializeField] private PlayerMovementData _movementData;
        [SerializeField] private CharacterAnimationAgent _animationAgent;

        private CharacterController _characterController;
        private Transform _cameraTransform;

        // ========== STATE MACHINE ==========
        private PlayerState _currentState;

        // Public state references for state transitions
        public IdleState IdleState { get; private set; }
        public WalkingState WalkingState { get; private set; }
        public RunningState RunningState { get; private set; }
        public CrouchingState CrouchingState { get; private set; }
        public AirborneState AirborneState { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool _showStateDebug = true;
        private string _currentStateName = "";

        // ========== MOVEMENT STATE ==========
        public Vector3 Velocity { get; set; }
        public bool IsGrounded { get; private set; } // legacy alias (kept for other systems)
        public bool IsWalking { get; set; }
        public bool IsSprinting { get; set; }

        // Crouch delegated (getter) + legacy setter (no rompe compilación si alguien lo setea)
        public bool IsCrouching
        {
            get => _crouchSystem?.IsCrouching ?? false;
            set
            {
                if (_crouchSystem == null) return;

                // Mantener compatibilidad: antes era set directo.
                // Usamos ForceCrouch para no "fallar silenciosamente" por falta de espacio.
                _crouchSystem.ForceCrouch(value);
            }
        }

        [Header("Grounding Filter")]
        [SerializeField, Min(0f)] private float groundedStableDelay = 0.06f;
        [SerializeField] private float fallingVerticalSpeedThreshold = -0.1f;

        private GroundProbeSystem _groundProbeSystem;
        private CrouchSystem _crouchSystem;

        // Grounding compatibility (delegated to GroundProbeSystem)
        public bool IsGroundedContact => _groundProbeSystem?.IsGroundedContact ?? false; // raw contact (custom probe)
        public bool IsGroundedStable => _groundProbeSystem?.IsGroundedStable ?? false;  // debounced grounded
        public bool IsFalling => _groundProbeSystem?.IsFalling ?? false;

        // Slope info (debug/animation/IK-ready)
        public Vector3 GroundNormal => _groundProbeSystem?.GroundNormal ?? Vector3.up;
        public float GroundAngle => _groundProbeSystem?.GroundAngle ?? 0f;
        public float GroundDistance => _groundProbeSystem?.GroundDistance ?? float.PositiveInfinity;

        // Locks
        public bool MovementLocked { get; private set; }     // global lock (cutscenes, etc.)
        private bool _landingLocked;                         // landing clip lock
        public bool CanJump { get; private set; } = true;

        // Jump one-frame flag (for animation)
        private bool _jumpPressedThisFrame;
        private float _verticalSpeed;

        // ========== VEHICLE ==========
        private IVehicle _currentVehicle;
        public IVehicle CurrentVehicle => _currentVehicle;
        public bool IsInVehicle => _currentVehicle != null;

        // ========== PUBLIC PROPERTIES ==========
        public PlayerMovementData MovementData => _movementData;
        public CharacterController CharacterController => _characterController;
        public Transform CameraTransform => _cameraTransform;
        public PlayerState CurrentState => _currentState;
        public string CurrentStateName => _currentStateName;

        // ICharacterAnimationSource
        float CharacterAnimationAgent.ICharacterAnimationSource.VerticalSpeed => _verticalSpeed;
        bool CharacterAnimationAgent.ICharacterAnimationSource.IsMovementLocked => IsMovementLocked;

        bool CharacterAnimationAgent.ICharacterAnimationSource.ConsumeJumpPressedThisFrame()
        {
            bool v = _jumpPressedThisFrame;
            _jumpPressedThisFrame = false;
            return v;
        }

        public bool IsMovementLocked => MovementLocked || _landingLocked;

        public Transform Transform => transform;

        // ========== UNITY LIFECYCLE ==========
        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            if (_animationAgent == null)
                _animationAgent = GetComponent<CharacterAnimationAgent>();

            if (Camera.main != null)
                _cameraTransform = Camera.main.transform;

            // Ground probe system
            _groundProbeSystem = new GroundProbeSystem(
                transform,
                _characterController,
                _movementData,
                groundedStableDelay,
                fallingVerticalSpeedThreshold
            );

            // Crouch system (POCO)
            _crouchSystem = new CrouchSystem(
                transform,
                _characterController,
                _movementData
            );

            // Initialize State Machine
            InitializeStateMachine();

            RefreshCanJump();
        }

        private void Start()
        {
            var thirdPersonCamera = FindFirstObjectByType<GTAFramework.GTACamera.Components.ThirdPersonCamera>();
            if (thirdPersonCamera != null)
                SetCameraTransform(thirdPersonCamera.transform);

            // Ensure first frame has correct grounded state even before any move
            _groundProbeSystem?.UpdateGrounding(_verticalSpeed, IsMovementLocked);
            IsGrounded = IsGroundedContact;
            RefreshCanJump();
        }

        private void Update()
        {
            _crouchSystem?.UpdateCollider();
            
        }

        private void LateUpdate()
        {
            UpdateStateMachine();
        }

        // ========== STATE MACHINE ==========
        private void InitializeStateMachine()
        {
            // Create all states
            IdleState = new IdleState(this);
            WalkingState = new WalkingState(this);
            RunningState = new RunningState(this);
            CrouchingState = new CrouchingState(this);
            AirborneState = new AirborneState(this);

            // Set initial state (Idle by default)
            _currentState = IdleState;
            _currentStateName = _currentState.GetStateName();
            _currentState.Enter();

            if (_showStateDebug)
                Debug.Log($"[PlayerController] State Machine initialized. Initial state: {_currentStateName}");
        }

        private void UpdateStateMachine()
        {
            if (_currentState == null)
                return;

            // Update current state
            _currentState.Update();

            // Check for transitions
            PlayerState nextState = _currentState.CheckTransitions();

            if (nextState != null && nextState != _currentState)
            {
                TransitionToState(nextState);
            }
        }

        private void TransitionToState(PlayerState newState)
        {
            if (newState == null || newState == _currentState)
                return;

            string previousStateName = _currentStateName;

            // Exit current state
            _currentState.Exit();

            // Enter new state
            _currentState = newState;
            _currentStateName = _currentState.GetStateName();
            _currentState.Enter();

            if (_showStateDebug)
                Debug.Log($"[PlayerController] State transition: {previousStateName} → {_currentStateName}");
        }

        /// <summary>
        /// Force a state transition (useful for external systems)
        /// </summary>
        public void ForceState(PlayerState newState)
        {
            if (newState != null)
                TransitionToState(newState);
        }

        /// <summary>
        /// Checks if there's enough space above the player to stand up.
        /// </summary>
        public bool CanStandUp()
        {
            return _crouchSystem?.CanStandUp() ?? true;
        }

        /// <summary>
        /// Intenta cambiar el estado de agachado.
        /// (Úsalo desde PlayerMovementSystem / Input para toggle crouch “seguro”)
        /// </summary>
        /// <param name="crouching">True para agacharse, false para levantarse.</param>
        /// <returns>True si el cambio fue exitoso.</returns>
        public bool TrySetCrouching(bool crouching)
        {
            return _crouchSystem?.SetCrouching(crouching) ?? false;
        }

        // ========== PUBLIC METHODS ==========
        public void Move(Vector3 velocityWorld)
        {
            if (IsMovementLocked)
                return;

            _characterController.Move(velocityWorld * Time.deltaTime);

            _groundProbeSystem?.UpdateGrounding(_verticalSpeed, IsMovementLocked);
            IsGrounded = IsGroundedContact; // legacy alias (kept for other systems)

            RefreshCanJump();
        }

        public void SetCameraTransform(Transform cameraTransform)
        {
            _cameraTransform = cameraTransform;
        }

        public void LockMovement(bool locked)
        {
            MovementLocked = locked;
            RefreshCanJump();
        }

        public void NotifyJump(float verticalSpeed)
        {
            _jumpPressedThisFrame = true;
            _verticalSpeed = verticalSpeed;

            // Si quieres que el coyote se corte inmediatamente al saltar:
            _groundProbeSystem?.ResetCoyoteTime();
        }

        public void SetVerticalSpeed(float verticalSpeed)
        {
            _verticalSpeed = verticalSpeed;
        }

        private void RefreshCanJump()
        {
            CanJump = IsGroundedStable && !IsMovementLocked && !IsCrouching;
        }

        // ========== ANIMATION EVENTS ==========
        // Animation Events (place these on the JumpLand clip)
        public void Anim_Land_Begin()
        {
            _landingLocked = true;
        }

        public void Anim_Land_End()
        {
            _landingLocked = false;
        }

        public void OnVehicleEnter(IVehicle vehicle)
        {
            _currentVehicle = vehicle;

            // Bloquear movimiento del jugador
            LockMovement(true);

            // Desactivar CharacterController (ya no necesita física de personaje)
            _characterController.enabled = false;

            // Desactivar animación de personaje
            if (_animationAgent != null)
                _animationAgent.enabled = false;

            // Asegurarse de que no esté agachado
            _crouchSystem?.ForceCrouch(false);

            Debug.Log($"[PlayerController] Entered vehicle: {vehicle.Transform.name}");
        }

        public void OnVehicleExit(IVehicle vehicle)
        {
            _currentVehicle = null;

            // Reactivar CharacterController
            _characterController.enabled = true;

            // Reactivar animación
            if (_animationAgent != null)
                _animationAgent.enabled = true;

            // Desbloquear movimiento
            LockMovement(false);

            // Resetear velocidad
            Velocity = Vector3.zero;
            _verticalSpeed = 0f;

            // Forzar estado Idle al salir
            ForceState(IdleState);

            Debug.Log($"[PlayerController] Exited vehicle: {vehicle.Transform.name}");
        }
    }
}
