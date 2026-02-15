using GTAFramework.GTACamera.Data;
using GTAFramework.GTACamera.Interfaces;
using UnityEngine;

namespace GTAFramework.GTACamera.States
{
    /// <summary>
    /// Estado normal de la cámara - comportamiento por defecto.
    /// Incluye zoom dinámico estilo GTA cuando miras hacia abajo.
    /// </summary>
    public class NormalCameraState : ICameraState
    {
        public string StateName => "Normal";

        // Contexto recibido en Enter
        protected StateTransitionContext _context;

        // Zoom dinámico calculado
        protected float _dynamicZoomOffset;

        public virtual void Enter(StateTransitionContext context)
        {
            _context = context;
            _dynamicZoomOffset = 0f;
        }

        public virtual void Exit()
        {
            // Limpieza si es necesaria
            _dynamicZoomOffset = 0f;
        }

        public virtual void Update(float deltaTime)
        {
            if (_context == null) return;

            var data = _context.RuntimeData;
            var settings = _context.Settings;

            // Suavizar distancia y altura
            data.CurrentDistance = Mathf.Lerp(
                data.CurrentDistance,
                data.TargetDistance,
                deltaTime * settings.distanceSmoothSpeed
            );
            data.CurrentHeight = Mathf.Lerp(
                data.CurrentHeight,
                data.TargetHeight,
                deltaTime * settings.heightSmoothSpeed
            );
        }

        public virtual void HandleRotation(Vector2 lookInput)
        {
            if (_context == null) return;

            var data = _context.RuntimeData;
            var settings = _context.Settings;

            // Rotación horizontal (yaw)
            data.CurrentYaw += lookInput.x * settings.horizontalSensitivity;

            // Rotación vertical (pitch) con clamp
            data.CurrentPitch -= lookInput.y * settings.verticalSensitivity;
            data.CurrentPitch = Mathf.Clamp(
                data.CurrentPitch,
                settings.minVerticalAngle,
                settings.maxVerticalAngle
            );

            // Calcular zoom dinámico usando la estrategia
            _dynamicZoomOffset = _context.ZoomStrategy.CalculateZoomOffset(
                data.CurrentPitch,
                data.TargetDistance,
                settings
            );
        }

        public virtual Vector3 GetDesiredPosition()
        {
            if (_context == null) return Vector3.zero;

            var data = _context.RuntimeData;
            var settings = _context.Settings;

            // Calcular posición basada en rotación
            Quaternion rotation = Quaternion.Euler(data.CurrentPitch, data.CurrentYaw, 0);
            float effectiveDistance = data.TargetDistance + _dynamicZoomOffset;

            Vector3 offset = rotation * new Vector3(0, data.TargetHeight, -effectiveDistance);
            Vector3 desiredPosition = _context.CameraPivot.position + offset;

            // Aplicar colisiones usando la estrategia
            return _context.CollisionHandler.HandleCollision(
                _context.CameraPivot.position,
                desiredPosition,
                settings.collisionRadius,
                settings.collisionLayers,
                settings.collisionOffset,
                settings.minDistance
            );
        }

        public virtual Quaternion GetDesiredRotation()
        {
            if (_context == null) return Quaternion.identity;
            return Quaternion.LookRotation(_context.CameraPivot.position - GetDesiredPosition());
        }
    }
}