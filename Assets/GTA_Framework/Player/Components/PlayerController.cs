using UnityEngine;
using GTAFramework.Player.Data;
using GTAFramework.GTA_Animation.Components;

namespace GTAFramework.Player.Components
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour, CharacterAnimationAgent.ICharacterAnimationSource
    {
        [Header("References")]
        [SerializeField] private PlayerMovementData _movementData;
        [SerializeField] private CharacterAnimationAgent _animationAgent;

        private CharacterController _characterController;
        private Transform _cameraTransform;

        // Estado del movimiento
        public Vector3 Velocity { get; set; }
        public bool IsGrounded { get; private set; }
        public bool IsWalking { get; set; }
        public bool IsSprinting { get; set; }
        public bool IsCrouching { get; set; }

        [Header("Grounding Filter")]
        [SerializeField, Min(0f)] private float groundedStableDelay = 0.06f;
        [SerializeField] private float fallingVerticalSpeedThreshold = -0.1f;

        public bool IsGroundedContact { get; private set; }
        public bool IsGroundedStable { get; private set; }
        public bool IsFalling { get; private set; }

        private float _groundedStableTimer;


        // Locks
        public bool MovementLocked { get; private set; }     // lock “global” (cutscenes, etc.)
        private bool _landingLocked;                         // lock específico por Landing clip

        public bool CanJump { get; private set; } = true;

        // Jump one-frame flag (para animación)
        private bool _jumpPressedThisFrame;
        private float _verticalSpeed;

        public PlayerMovementData MovementData => _movementData;
        public CharacterController CharacterController => _characterController;
        public Transform CameraTransform => _cameraTransform;

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

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            if (_animationAgent == null)
                _animationAgent = GetComponent<CharacterAnimationAgent>();

            if (Camera.main != null)
                _cameraTransform = Camera.main.transform;

            RefreshCanJump();
        }

        private void Start()
        {
            var thirdPersonCamera = FindFirstObjectByType<GTAFramework.GTACamera.Components.ThirdPersonCamera>();
            if (thirdPersonCamera != null)
                SetCameraTransform(thirdPersonCamera.transform);
        }

        private void PostMoveUpdate()
        {
            IsGroundedContact = _characterController.isGrounded;
            IsGrounded = IsGroundedContact; // si aún usas IsGrounded en otros lados

            if (IsGroundedContact)
            {
                _groundedStableTimer += Time.deltaTime;
            }
            else
            {
                _groundedStableTimer = 0f;
            }

            IsGroundedStable = IsGroundedContact && _groundedStableTimer >= groundedStableDelay;

            // Falling “real”: no grounded + ya voy hacia abajo
            IsFalling = !IsGroundedContact && _verticalSpeed < fallingVerticalSpeedThreshold;

            RefreshCanJump();
        }


        private void RefreshCanJump()
        {
            CanJump = IsGroundedStable && !IsMovementLocked;
        }

        public void Move(Vector3 velocityWorld)
        {
            if (IsMovementLocked)
                return;

            _characterController.Move(velocityWorld * Time.deltaTime);
            PostMoveUpdate();
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
            // Esto NO dispara trigger del Animator. Solo activa un bool one-frame para el Agent.
            _jumpPressedThisFrame = true;
            _verticalSpeed = verticalSpeed;
        }

        public void SetVerticalSpeed(float verticalSpeed)
        {
            _verticalSpeed = verticalSpeed;
        }

        // ----------------------------------------------------
        // Animation Events (ponlos en el clip JumpLand)
        // ----------------------------------------------------

        // Llamar al inicio del clip/state JumpLand
        public void Anim_Land_Begin()
        {
            _landingLocked = true;
            RefreshCanJump();
        }

        // Llamar al final del clip/state JumpLand
        public void Anim_Land_End()
        {
            _landingLocked = false;
            RefreshCanJump();
        }
    }
}
