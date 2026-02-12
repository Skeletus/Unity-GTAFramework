using UnityEngine;
using GTAFramework.Player.Data;

namespace GTAFramework.Player.Components
{
    /// <summary>
    /// Sistema de detección de suelo extraído de PlayerController.
    /// Maneja ground probing, snap-to-ground, coyote time y detección de caída.
    /// </summary>
    public class GroundProbeSystem
    {
        // ===== CONFIGURACIÓN =====
        private readonly float _groundedStableDelay;
        private readonly float _fallingVerticalSpeedThreshold;

        // ===== ESTADO =====
        /// <summary>Contacto crudo con el suelo (resultado directo del probe).</summary>
        public bool IsGroundedContact { get; private set; }

        /// <summary>Grounded con filtro temporal (debounced) para evitar flicker.</summary>
        public bool IsGroundedStable { get; private set; }

        /// <summary>True cuando ya no cuenta como grounded (incluye coyote) y cae con velocidad vertical suficiente.</summary>
        public bool IsFalling { get; private set; }

        /// <summary>Normal del suelo detectado (útil para animación/IK y slopes).</summary>
        public Vector3 GroundNormal { get; private set; } = Vector3.up;

        /// <summary>Ángulo del suelo (grados) respecto a Vector3.up.</summary>
        public float GroundAngle { get; private set; }

        /// <summary>Distancia desde el cast hasta el suelo.</summary>
        public float GroundDistance { get; private set; } = float.PositiveInfinity;

        private float _groundedStableTimer;
        private float _coyoteTimer;

        // ===== REFERENCIAS =====
        private readonly CharacterController _characterController;
        private readonly PlayerMovementData _movementData;
        private readonly Transform _transform;

        // ===== PROPIEDADES PÚBLICAS =====
        /// <summary>Tiempo restante de coyote (segundos).</summary>
        public float CoyoteTimeRemaining => _coyoteTimer;

        /// <summary>True si aún queda coyote time.</summary>
        public bool HasCoyoteTime => _coyoteTimer > 0f;

        // ===== CONSTRUCTOR =====
        /// <summary>
        /// Crea un sistema de probing de suelo sin depender de MonoBehaviour.
        /// </summary>
        public GroundProbeSystem(
            Transform transform,
            CharacterController characterController,
            PlayerMovementData movementData,
            float groundedStableDelay = 0.06f,
            float fallingVerticalSpeedThreshold = -0.1f)
        {
            _transform = transform;
            _characterController = characterController;
            _movementData = movementData;
            _groundedStableDelay = Mathf.Max(0f, groundedStableDelay);
            _fallingVerticalSpeedThreshold = fallingVerticalSpeedThreshold;

            IsGroundedContact = false;
            IsGroundedStable = false;
            IsFalling = false;
        }

        // ===== MÉTODOS PÚBLICOS =====

        /// <summary>
        /// Actualiza el estado de grounding. Llamar después de cada <see cref="CharacterController.Move"/>.
        /// </summary>
        /// <param name="verticalSpeed">Velocidad vertical actual (m/s).</param>
        /// <param name="isMovementLocked">Si el movimiento está bloqueado (evita snap cuando está locked).</param>
        public void UpdateGrounding(float verticalSpeed, bool isMovementLocked)
        {
            float dt = Time.deltaTime;

            // Sin dependencias válidas, deja estado "en aire" seguro.
            if (_movementData == null || _characterController == null || _transform == null)
            {
                IsGroundedContact = false;
                IsGroundedStable = false;
                IsFalling = verticalSpeed < _fallingVerticalSpeedThreshold;

                GroundNormal = Vector3.up;
                GroundAngle = 0f;
                GroundDistance = float.PositiveInfinity;

                _groundedStableTimer = 0f;
                _coyoteTimer = Mathf.Max(0f, _coyoteTimer - dt);
                return;
            }

            // 1) Probe ground con SphereCast (mejor para rampas/escaleras que isGrounded)
            bool groundedProbe = ProbeGround(out RaycastHit hit);

            IsGroundedContact = groundedProbe;

            if (IsGroundedContact)
            {
                _groundedStableTimer += dt;
                _coyoteTimer = _movementData.coyoteTime;

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

            IsGroundedStable = IsGroundedContact && _groundedStableTimer >= _groundedStableDelay;

            // 2) Snap-down al bajar ramps/stairs para evitar falsas caídas.
            if (!IsGroundedContact && !isMovementLocked && verticalSpeed <= 0f)
            {
                if (TrySnapToGround(out RaycastHit snapHit))
                {
                    IsGroundedContact = true;

                    _groundedStableTimer += dt;
                    _coyoteTimer = _movementData.coyoteTime;

                    GroundNormal = snapHit.normal;
                    GroundAngle = Vector3.Angle(snapHit.normal, Vector3.up);
                    GroundDistance = snapHit.distance;

                    IsGroundedStable = IsGroundedContact && _groundedStableTimer >= _groundedStableDelay;
                }
            }

            // 3) Falling "real": no grounded (incluye coyote) + ya va hacia abajo
            bool hasCoyote = _coyoteTimer > 0f;
            bool groundedForFalling = IsGroundedContact || hasCoyote;
            IsFalling = !groundedForFalling && verticalSpeed < _fallingVerticalSpeedThreshold;

            if (_coyoteTimer < 0f)
                _coyoteTimer = 0f;
        }

        /// <summary>
        /// Resetea el coyote timer (llamar al saltar).
        /// </summary>
        public void ResetCoyoteTime()
        {
            _coyoteTimer = 0f;
        }

        // ===== MÉTODOS PRIVADOS =====

        private bool ProbeGround(out RaycastHit hit)
        {
            hit = default;

            if (_movementData == null || _characterController == null || _transform == null)
                return false;

            // Capsule bottom in world space
            Vector3 centerWorld = _transform.TransformPoint(_characterController.center);
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

            if (_movementData == null || _characterController == null || _transform == null)
                return false;

            Vector3 centerWorld = _transform.TransformPoint(_characterController.center);
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
    }
}
