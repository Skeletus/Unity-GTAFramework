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

        // Movement state
        public Vector3 Velocity { get; set; }
        public bool IsGrounded { get; private set; } // legacy alias (kept for other systems)
        public bool IsWalking { get; set; }
        public bool IsSprinting { get; set; }
        public bool IsCrouching { get; set; }

        [Header("Grounding Filter")]
        [SerializeField, Min(0f)] private float groundedStableDelay = 0.06f;
        [SerializeField] private float fallingVerticalSpeedThreshold = -0.1f;

        public bool IsGroundedContact { get; private set; } // raw contact (custom probe)
        public bool IsGroundedStable { get; private set; }  // debounced grounded
        public bool IsFalling { get; private set; }

        // Slope info (debug/animation/IK-ready)
        public Vector3 GroundNormal { get; private set; } = Vector3.up;
        public float GroundAngle { get; private set; }
        public float GroundDistance { get; private set; }

        private float _groundedStableTimer;
        private float _coyoteTimer;

        // Locks
        public bool MovementLocked { get; private set; }     // global lock (cutscenes, etc.)
        private bool _landingLocked;                         // landing clip lock
        public bool CanJump { get; private set; } = true;

        // Jump one-frame flag (for animation)
        private bool _jumpPressedThisFrame;
        private float _verticalSpeed;

        // Character Controller original values
        private float _originalHeight;
        private float _originalCenterY;
        private float _currentHeight;
        private float _currentCenterY;



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

            // Store original CharacterController values
            _originalHeight = _characterController.height;
            _originalCenterY = _characterController.center.y;
            _currentHeight = _originalHeight;
            _currentCenterY = _originalCenterY;



            RefreshCanJump();
        }

        private void Start()
        {
            var thirdPersonCamera = FindFirstObjectByType<GTAFramework.GTACamera.Components.ThirdPersonCamera>();
            if (thirdPersonCamera != null)
                SetCameraTransform(thirdPersonCamera.transform);

            // Ensure first frame has correct grounded state even before any move
            PostMoveUpdate();
        }

        private void Update()
        {
            UpdateCrouchCollider();
        }

        /// <summary>
        /// Updates the CharacterController collider based on crouch state
        /// </summary>
        private void UpdateCrouchCollider()
        {
            if (_movementData == null || _characterController == null)
                return;

            float targetHeight;
            float targetCenterY;

            if (IsCrouching)
            {
                // Calculate crouched height and center
                targetHeight = _originalHeight * _movementData.crouchHeightMultiplier;
                // Adjust center Y so the bottom of the capsule stays at the same position
                float heightDifference = _originalHeight - targetHeight;
                targetCenterY = _originalCenterY - (heightDifference * 0.5f);
            }
            else
            {
                // Return to original values
                targetHeight = _originalHeight;
                targetCenterY = _originalCenterY;

            }

            // Smoothly transition
            float transitionSpeed = _movementData.crouchTransitionSpeed * Time.deltaTime;
            _currentHeight = Mathf.Lerp(_currentHeight, targetHeight, transitionSpeed);
            _currentCenterY = Mathf.Lerp(_currentCenterY, targetCenterY, transitionSpeed);

            // Apply to CharacterController
            _characterController.height = _currentHeight;
            Vector3 center = _characterController.center;
            center.y = _currentCenterY;
            _characterController.center = center;
        }

        /// <summary>
        /// Checks if there's enough space above the player to stand up
        /// </summary>
        public bool CanStandUp()
        {
            if (!IsCrouching)
                return true;

            float checkHeight = _originalHeight;
            float currentHeight = _characterController.height;
            float additionalHeight = checkHeight - currentHeight;

            if (additionalHeight <= 0.01f)
                return true;

            // Get the current position of the character controller
            Vector3 centerLocal = _characterController.center;
            Vector3 centerWorld = transform.TransformPoint(centerLocal);
            float radius = _characterController.radius * 0.95f;

            // Calculate the top point of the current crouched capsule
            float currentHalfHeight = currentHeight * 0.5f;
            Vector3 currentTop = centerWorld + Vector3.up * (currentHalfHeight - radius);

            // Distance to check (from current top to where standing top would be)
            float checkDistance = additionalHeight + 0.1f;

            Debug.Log($"CanStandUp Check - Current Height: {currentHeight:F2}, Target Height: {checkHeight:F2}, Check Distance: {checkDistance:F2}");
            Debug.Log($"Center World: {centerWorld}, Current Top: {currentTop}, Radius: {radius:F2}");


            // Perform a sphere cast upward from current top position
            bool hasObstacle = Physics.SphereCast(
                currentTop,
                radius,
                Vector3.up,
                out RaycastHit hit,
                checkDistance,
                _movementData.groundMask,
                QueryTriggerInteraction.Ignore
            );

            if (hasObstacle)
            {
                Debug.Log($"<color=red>OBSTACULO DETECTADO: {hit.collider.gameObject.name} a distancia {hit.distance:F2}</color>");
            }
            else
            {
                Debug.Log($"<color=green>NO HAY OBSTACULOS - Puede levantarse</color>");
            }

            // Debug visualization
            Debug.DrawRay(currentTop, Vector3.up * checkDistance, hasObstacle ? Color.red : Color.green, 0.5f);

            return !hasObstacle;

        }



        /// <summary>
        /// Robust grounding:
        /// - Uses a SphereCast under the capsule instead of only CharacterController.isGrounded
        /// - Adds stable delay + coyote time (prevents false "falling" on ramps/stairs)
        /// - Snaps down a little when walking down slopes so you don't lose contact
        /// </summary>
        private void PostMoveUpdate()
        {
            float dt = Time.deltaTime;

            // 1) Probe ground with a SphereCast (works on ramps/stairs better than isGrounded)
            bool groundedProbe = ProbeGround(out RaycastHit hit);

            IsGroundedContact = groundedProbe;
            IsGrounded = IsGroundedContact; // keep compatibility

            if (IsGroundedContact)
            {
                _groundedStableTimer += dt;
                _coyoteTimer = _movementData != null ? _movementData.coyoteTime : 0f;

                GroundNormal = hit.normal;
                GroundAngle = Vector3.Angle(hit.normal, Vector3.up);
                GroundDistance = hit.distance;
            }
            else
            {
                _groundedStableTimer = 0f;
                _coyoteTimer -= dt;

                GroundNormal = Vector3.up;
                GroundAngle = 0f;
                GroundDistance = float.PositiveInfinity;
            }

            IsGroundedStable = IsGroundedContact && _groundedStableTimer >= groundedStableDelay;

            // 2) Snap-down when going down ramps/stairs:
            // if we are moving down and very close to valid ground, snap to it (prevents false falling).
            if (!IsGroundedContact && !IsMovementLocked && _verticalSpeed <= 0f)
            {
                if (TrySnapToGround(out RaycastHit snapHit))
                {
                    IsGroundedContact = true;
                    IsGrounded = true;

                    _groundedStableTimer += dt;
                    _coyoteTimer = _movementData != null ? _movementData.coyoteTime : 0f;

                    GroundNormal = snapHit.normal;
                    GroundAngle = Vector3.Angle(snapHit.normal, Vector3.up);
                    GroundDistance = snapHit.distance;

                    IsGroundedStable = IsGroundedContact && _groundedStableTimer >= groundedStableDelay;
                }
            }

            // 3) Falling "real": not grounded (including coyote) + already moving down
            bool hasCoyote = _coyoteTimer > 0f;
            bool groundedForFalling = IsGroundedContact || hasCoyote;
            IsFalling = !groundedForFalling && _verticalSpeed < fallingVerticalSpeedThreshold;

            RefreshCanJump();
        }

        private bool ProbeGround(out RaycastHit hit)
        {
            hit = default;

            if (_movementData == null || _characterController == null)
                return false;

            // Capsule bottom in world space
            Vector3 centerWorld = transform.TransformPoint(_characterController.center);
            float radius = Mathf.Max(0.001f, _characterController.radius);
            float height = Mathf.Max(radius * 2f, _characterController.height);

            float castRadius = radius * Mathf.Clamp(_movementData.groundProbeRadiusFactor, 0.5f, 1.0f);
            float bottomOffset = (height * 0.5f) - radius;
            Vector3 bottom = centerWorld + Vector3.down * bottomOffset;

            // Start a bit above bottom to avoid immediate overlaps
            Vector3 origin = bottom + Vector3.up * 0.05f;

            float castDistance = 0.05f + _movementData.groundProbeDistance;

            bool hasHit = Physics.SphereCast(
                origin,
                castRadius,
                Vector3.down,
                out hit,
                castDistance,
                _movementData.groundMask,
                QueryTriggerInteraction.Ignore
            );

            if (!hasHit)
                return false;

            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle > _characterController.slopeLimit + 0.01f)
                return false;

            return true;
        }

        private bool TrySnapToGround(out RaycastHit hit)
        {
            hit = default;

            if (_movementData == null || _characterController == null)
                return false;

            float snapDist = _movementData.groundSnapDistance;
            if (snapDist <= 0f)
                return false;

            if (!ProbeGroundForDistance(snapDist, out hit))
                return false;

            float epsilon = 0.001f;
            float moveDown = Mathf.Max(0f, hit.distance - epsilon);

            if (moveDown <= 0f)
                return false;

            _characterController.Move(Vector3.down * moveDown);
            return true;
        }

        private bool ProbeGroundForDistance(float extraDistance, out RaycastHit hit)
        {
            hit = default;

            if (_movementData == null || _characterController == null)
                return false;

            Vector3 centerWorld = transform.TransformPoint(_characterController.center);
            float radius = Mathf.Max(0.001f, _characterController.radius);
            float height = Mathf.Max(radius * 2f, _characterController.height);

            float castRadius = radius * Mathf.Clamp(_movementData.groundProbeRadiusFactor, 0.5f, 1.0f);
            float bottomOffset = (height * 0.5f) - radius;
            Vector3 bottom = centerWorld + Vector3.down * bottomOffset;
            Vector3 origin = bottom + Vector3.up * 0.05f;

            float castDistance = 0.05f + extraDistance;

            bool hasHit = Physics.SphereCast(
                origin,
                castRadius,
                Vector3.down,
                out hit,
                castDistance,
                _movementData.groundMask,
                QueryTriggerInteraction.Ignore
            );

            if (!hasHit)
                return false;

            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle > _characterController.slopeLimit + 0.01f)
                return false;

            return true;
        }

        public void Move(Vector3 velocityWorld)
        {
            // Even if locked, we still update grounded/falling correctly (ramps, landing, etc.)
            if(IsMovementLocked)
            {
                return;
            }
            if (!IsMovementLocked)
            {
                _characterController.Move(velocityWorld * Time.deltaTime);
            }

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
            _jumpPressedThisFrame = true;
            _verticalSpeed = verticalSpeed;
        }

        public void SetVerticalSpeed(float verticalSpeed)
        {
            _verticalSpeed = verticalSpeed;
        }

        private void RefreshCanJump()
        {
            CanJump = IsGroundedStable && !IsMovementLocked && !IsCrouching;
        }

        // ----------------------------------------------------
        // Animation Events (place these on the JumpLand clip)
        // ----------------------------------------------------
        public void Anim_Land_Begin()
        {
            _landingLocked = true;
            RefreshCanJump();
        }

        public void Anim_Land_End()
        {
            _landingLocked = false;
            RefreshCanJump();
        }
    }
}
