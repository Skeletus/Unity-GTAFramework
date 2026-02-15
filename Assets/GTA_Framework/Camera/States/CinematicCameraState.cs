using GTAFramework.GTACamera.Data;
using GTAFramework.GTACamera.Interfaces;
using UnityEngine;

namespace GTAFramework.GTACamera.States
{
    /// <summary>
    /// Estado cinemático - para cutscenes y secuencias scripteadas.
    /// Desactiva el control del jugador y permite animaciones de cámara.
    /// </summary>
    public class CinematicCameraState : ICameraState
    {
        public string StateName => "Cinematic";

        private StateTransitionContext _context;

        // Configuración cinemática
        private const float CINEMATIC_DISTANCE = 4f;
        private const float CINEMATIC_HEIGHT = 2.5f;
        private const float CINEMATIC_SMOOTH_SPEED = 3f;

        // Datos de la secuencia cinemática
        private Transform _lookAtTarget;
        private Vector3 _offset;
        private bool _isActive;

        public void Enter(StateTransitionContext context)
        {
            _context = context;
            _isActive = true;

            // Configuración inicial cinemática
            _context.RuntimeData.TargetDistance = CINEMATIC_DISTANCE;
            _context.RuntimeData.TargetHeight = CINEMATIC_HEIGHT;
        }

        public void Exit()
        {
            _isActive = false;
            _lookAtTarget = null;
        }

        /// <summary>
        /// Configura el objetivo de la cámara cinemática.
        /// </summary>
        public void SetLookAtTarget(Transform target, Vector3 offset)
        {
            _lookAtTarget = target;
            _offset = offset;
        }

        public void Update(float deltaTime)
        {
            if (_context == null || !_isActive) return;

            var data = _context.RuntimeData;
            var settings = _context.Settings;

            // Suavizado más lento para efecto cinemático
            data.CurrentDistance = Mathf.Lerp(
                data.CurrentDistance,
                data.TargetDistance,
                deltaTime * CINEMATIC_SMOOTH_SPEED
            );
            data.CurrentHeight = Mathf.Lerp(
                data.CurrentHeight,
                data.TargetHeight,
                deltaTime * CINEMATIC_SMOOTH_SPEED
            );
        }

        public void HandleRotation(Vector2 lookInput)
        {
            // En modo cinemático, el input del jugador se ignora
            // La rotación es controlada por scripts externos o animaciones
        }

        public Vector3 GetDesiredPosition()
        {
            if (_context == null) return Vector3.zero;

            var data = _context.RuntimeData;
            var settings = _context.Settings;

            // Si hay un lookAtTarget, usarlo
            Vector3 pivotPosition = _lookAtTarget != null
                ? _lookAtTarget.position + _offset
                : _context.CameraPivot.position;

            // Posición fija o animada (sin zoom dinámico)
            Quaternion rotation = Quaternion.Euler(data.CurrentPitch, data.CurrentYaw, 0);
            Vector3 offset = rotation * new Vector3(0, data.TargetHeight, -data.TargetDistance);
            Vector3 desiredPosition = pivotPosition + offset;

            return _context.CollisionHandler.HandleCollision(
                pivotPosition,
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

            // Mirar al objetivo cinemático o al pivot
            Vector3 lookTarget = _lookAtTarget != null
                ? _lookAtTarget.position + _offset
                : _context.CameraPivot.position;

            return Quaternion.LookRotation(lookTarget - GetDesiredPosition());
        }

        /// <summary>
        /// Permite controlar la rotación desde un script externo.
        /// </summary>
        public void SetRotation(float yaw, float pitch)
        {
            if (_context == null) return;
            _context.RuntimeData.CurrentYaw = yaw;
            _context.RuntimeData.CurrentPitch = Mathf.Clamp(
                pitch,
                _context.Settings.minVerticalAngle,
                _context.Settings.maxVerticalAngle
            );
        }
    }
}