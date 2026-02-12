using UnityEngine;
using GTAFramework.Player.Data;

namespace GTAFramework.Player.Components
{
    /// <summary>
    /// Sistema de agachado extraído de PlayerController.
    /// Maneja el cambio de altura del CharacterController, verificación de espacio
    /// para levantarse, y transiciones suaves entre estados.
    /// </summary>
    public class CrouchSystem
    {
        // ===== CONFIGURACIÓN =====
        private readonly PlayerMovementData _movementData;
        private readonly CharacterController _characterController;
        private readonly Transform _transform;

        // ===== ESTADO ORIGINAL =====
        private readonly float _originalHeight;
        private readonly float _originalCenterY;

        // ===== ESTADO ACTUAL =====
        private float _currentHeight;
        private float _currentCenterY;

        // ===== PROPIEDADES PÚBLICAS =====
        /// <summary>True si el personaje está agachado.</summary>
        public bool IsCrouching { get; private set; }

        /// <summary>Altura original del CharacterController (de pie).</summary>
        public float OriginalHeight => _originalHeight;

        /// <summary>Altura actual del collider.</summary>
        public float CurrentHeight => _currentHeight;

        // ===== CONSTANTES =====
        private const float HEIGHT_TOLERANCE = 0.01f;

        // ===== CONSTRUCTOR =====
        /// <summary>
        /// Crea un sistema de agachado sin depender de MonoBehaviour.
        /// </summary>
        public CrouchSystem(
            Transform transform,
            CharacterController characterController,
            PlayerMovementData movementData)
        {
            _transform = transform;
            _characterController = characterController;
            _movementData = movementData;

            // Store original values
            _originalHeight = _characterController.height;
            _originalCenterY = _characterController.center.y;
            _currentHeight = _originalHeight;
            _currentCenterY = _originalCenterY;

            IsCrouching = false;
        }

        // ===== MÉTODOS PÚBLICOS =====

        /// <summary>
        /// Establece el estado de agachado.
        /// </summary>
        /// <param name="crouching">True para agacharse, false para levantarse.</param>
        /// <returns>True si el cambio de estado fue exitoso.</returns>
        public bool SetCrouching(bool crouching)
        {
            // Si quiere levantarse, verificar si hay espacio
            if (!crouching && IsCrouching)
            {
                if (!CanStandUp())
                    return false;
            }

            IsCrouching = crouching;
            return true;
        }

        /// <summary>
        /// Intenta alternar el estado de agachado.
        /// </summary>
        /// <returns>True si el toggle fue exitoso.</returns>
        public bool ToggleCrouch()
        {
            return SetCrouching(!IsCrouching);
        }

        /// <summary>
        /// Actualiza el collider del CharacterController. Llamar en Update().
        /// </summary>
        public void UpdateCollider()
        {
            if (_movementData == null || _characterController == null)
                return;

            float targetHeight;
            float targetCenterY;

            if (IsCrouching)
            {
                targetHeight = _originalHeight * _movementData.crouchHeightMultiplier;
                float heightDifference = _originalHeight - targetHeight;
                targetCenterY = _originalCenterY - (heightDifference * 0.5f);
            }
            else
            {
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
        /// Verifica si hay suficiente espacio arriba para levantarse.
        /// </summary>
        /// <returns>True si el personaje puede ponerse de pie.</returns>
        public bool CanStandUp()
        {
            if (!IsCrouching)
                return true;

            if (_characterController == null || _movementData == null)
                return false;

            float currentHeight = _characterController.height;
            float heightDifference = _originalHeight - currentHeight;

            if (heightDifference <= HEIGHT_TOLERANCE)
                return true;

            // Punto del tope actual del capsule (en mundo) para "probar" hacia arriba
            Vector3 centerWorld = _transform.TransformPoint(_characterController.center);
            float currentHalfHeight = currentHeight * 0.5f;
            float radius = _characterController.radius * 0.95f;
            Vector3 currentTop = centerWorld + Vector3.up * (currentHalfHeight - radius);

            float checkDistance = heightDifference + 0.1f;

            bool hasObstacle = Physics.SphereCast(
                currentTop,
                radius,
                Vector3.up,
                out _,
                checkDistance,
                _movementData.groundMask,
                QueryTriggerInteraction.Ignore
            );

            return !hasObstacle;
        }

        /// <summary>
        /// Fuerza el estado de agachado sin verificar espacio (usar con cuidado).
        /// </summary>
        public void ForceCrouch(bool crouching)
        {
            IsCrouching = crouching;
        }
    }
}
