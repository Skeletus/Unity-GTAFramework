using GTAFramework.GTACamera.Data;
using GTAFramework.GTACamera.Interfaces;
using UnityEngine;

namespace GTAFramework.GTACamera.States
{
    /// <summary>
    /// Estado de apuntado - cámara más cerca y con menor sensibilidad.
    /// Incluye offset lateral para "shoulder aiming".
    /// </summary>
    public class AimingCameraState : ICameraState
    {
        public string StateName => "Aiming";

        private StateTransitionContext _context;

        // Configuración del estado aiming
        private const float AIM_DISTANCE_FACTOR = 0.5f;  // 50% de la distancia normal
        private const float AIM_HEIGHT_FACTOR = 0.7f;    // 70% de la altura normal
        private const float AIM_SENSITIVITY_FACTOR = 0.7f; // 70% de la sensibilidad
        private const float AIM_SMOOTH_MULTIPLIER = 1.5f; // Transiciones más rápidas
        private const float SHOULDER_OFFSET = 0.5f;      // Offset lateral para shoulder aim

        public void Enter(StateTransitionContext context)
        {
            _context = context;

            // Aplicar zoom in automático al entrar en modo aiming
            _context.RuntimeData.TargetDistance = _context.Settings.normalDistance * AIM_DISTANCE_FACTOR;
            _context.RuntimeData.TargetHeight = _context.Settings.normalHeight * AIM_HEIGHT_FACTOR;
        }

        public void Exit()
        {
            // Restaurar valores normales al salir
            _context.RuntimeData.TargetDistance = _context.Settings.normalDistance;
            _context.RuntimeData.TargetHeight = _context.Settings.normalHeight;
        }

        public void Update(float deltaTime)
        {
            if (_context == null) return;

            var data = _context.RuntimeData;
            var settings = _context.Settings;

            // Suavizado más rápido para aiming (más responsivo)
            data.CurrentDistance = Mathf.Lerp(
                data.CurrentDistance,
                data.TargetDistance,
                deltaTime * settings.distanceSmoothSpeed * AIM_SMOOTH_MULTIPLIER
            );
            data.CurrentHeight = Mathf.Lerp(
                data.CurrentHeight,
                data.TargetHeight,
                deltaTime * settings.heightSmoothSpeed * AIM_SMOOTH_MULTIPLIER
            );
        }

        public void HandleRotation(Vector2 lookInput)
        {
            if (_context == null) return;

            var data = _context.RuntimeData;
            var settings = _context.Settings;

            // Sensibilidad reducida para mayor precisión
            data.CurrentYaw += lookInput.x * settings.horizontalSensitivity * AIM_SENSITIVITY_FACTOR;
            data.CurrentPitch -= lookInput.y * settings.verticalSensitivity * AIM_SENSITIVITY_FACTOR;
            data.CurrentPitch = Mathf.Clamp(
                data.CurrentPitch,
                settings.minVerticalAngle,
                settings.maxVerticalAngle
            );
        }

        public Vector3 GetDesiredPosition()
        {
            if (_context == null) return Vector3.zero;

            var data = _context.RuntimeData;
            var settings = _context.Settings;

            // Posición con offset lateral (shoulder aim)
            Quaternion rotation = Quaternion.Euler(data.CurrentPitch, data.CurrentYaw, 0);
            Vector3 offset = rotation * new Vector3(SHOULDER_OFFSET, data.TargetHeight, -data.TargetDistance);
            Vector3 desiredPosition = _context.CameraPivot.position + offset;

            return _context.CollisionHandler.HandleCollision(
                _context.CameraPivot.position,
                desiredPosition,
                settings.collisionRadius,
                settings.collisionLayers,
                settings.collisionOffset,
                settings.minDistance
            );
        }

        public Quaternion GetDesiredRotation()
        {
            if (_context == null) return Quaternion.identity;
            return Quaternion.LookRotation(_context.CameraPivot.position - GetDesiredPosition());
        }
    }
}