using UnityEngine;
using GTAFramework.GTACamera.Data;
using GTAFramework.GTACamera.Interfaces;
using GTAFramework.GTACamera.Strategies.Collision;
using GTAFramework.GTACamera.Strategies.Zoom;
using System.Collections.Generic;
using GTAFramework.GTACamera.States;

namespace GTAFramework.GTACamera.Components
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CameraSettingsData _cameraSettings;
        [SerializeField] private Transform _target;
        [SerializeField] private Transform _cameraPivot; // Punto donde la cámara mira

        [Header("Strategies")]
        [SerializeReference] private ICameraCollisionHandler _collisionHandler;
        [SerializeReference] private IZoomStrategy _zoomStrategy;

        // State Pattern
        private ICameraState _currentState;
        private Dictionary<string, ICameraState> _states;
        private StateTransitionContext _context;
        private CameraRuntimeData _runtimeData;

        private UnityEngine.Camera _camera;

        // Propiedades públicas
        public CameraSettingsData Settings => _cameraSettings;
        public Transform Target => _target;
        public Transform CameraPivot => _cameraPivot;
        public float CurrentYaw => _runtimeData?.CurrentYaw ?? 0f;
        public float CurrentPitch => _runtimeData?.CurrentPitch ?? 0f;
        public string CurrentStateName => _currentState?.StateName ?? "None";
        public CameraRuntimeData RuntimeData => _runtimeData;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();

            if (_camera == null)
            {
                Debug.LogError("ThirdPersonCamera: Camera component not found!");
            }

            // Inicializar estrategias
            _collisionHandler ??= new SphereCastCollisionHandler();
            _zoomStrategy ??= new DownwardZoomStrategy();

            // 1. PRIMERO: Inicializar runtime data
            InitializeRuntimeData();

            // 2. SEGUNDO: Crear pivot (ANTES de inicializar estados)
            CreateOrFindPivot();

            // 3. TERCERO: Inicializar estados (después del pivot)
            InitializeStates();
        }

        private void InitializeRuntimeData()
        {
            _runtimeData = new CameraRuntimeData();

            if (_cameraSettings != null)
            {
                _runtimeData.ResetToDefault(_cameraSettings);
            }
        }

        private void InitializeStates()
        {
            // Crear contexto de transición
            _context = new StateTransitionContext
            {
                Settings = _cameraSettings,
                RuntimeData = _runtimeData,
                ZoomStrategy = _zoomStrategy,
                CollisionHandler = _collisionHandler,
                Target = _target,
                CameraPivot = _cameraPivot
            };

            // Crear diccionario de estados
            _states = new Dictionary<string, ICameraState>
            {
                { "Normal", new NormalCameraState() },
                { "Aiming", new AimingCameraState() },
                { "Cinematic", new CinematicCameraState() }
            };

            // Entrar al estado inicial
            SetState("Normal");
        }

        private void CreateOrFindPivot()
        {
            if (_target == null) return;

            var existingPivot = _target.Find("CameraPivot");
            if (existingPivot != null)
            {
                _cameraPivot = existingPivot;
            }
            else if (_cameraPivot == null)
            {
                var pivotObj = new GameObject("CameraPivot");
                _cameraPivot = pivotObj.transform;
                _cameraPivot.SetParent(_target);
                _cameraPivot.localPosition = Vector3.up * _cameraSettings.pivotYOffset;
            }
        }

        /// <summary>
        /// Cambia el estado actual de la cámara.
        /// </summary>
        /// <param name="stateName">Nombre del estado: "Normal", "Aiming", "Cinematic"</param>
        public void SetState(string stateName)
        {
            if (!_states.TryGetValue(stateName, out var newState))
            {
                Debug.LogWarning($"ThirdPersonCamera: State '{stateName}' not found!");
                return;
            }

            // Salir del estado actual
            _currentState?.Exit();

            // Cambiar al nuevo estado
            _currentState = newState;

            // Actualizar contexto con valores actuales
            _context.Target = _target;
            _context.CameraPivot = _cameraPivot;

            // Entrar al nuevo estado
            _currentState.Enter(_context);

            Debug.Log($"ThirdPersonCamera: Changed to state '{stateName}'");
        }

        public void SetTarget(Transform target)
        {
            _target = target;
            CreateOrFindPivot();

            // Actualizar contexto
            if (_context != null)
            {
                _context.Target = _target;
                _context.CameraPivot = _cameraPivot;
            }
        }

        public void RotateCamera(Vector2 lookInput)
        {
            // Delegar al estado actual
            _currentState?.HandleRotation(lookInput);
        }

        public void UpdateCameraPosition()
        {
            if (_target == null || _cameraPivot == null || _cameraSettings == null || _currentState == null) return;

            // El estado actualiza su lógica interna
            _currentState?.Update(Time.deltaTime);

            // Obtener posición y rotación del estado
            Vector3 desiredPosition = _currentState.GetDesiredPosition();
            Quaternion desiredRotation = _currentState.GetDesiredRotation();

            // Aplicar con suavizado
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                _cameraSettings.positionSmoothSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                _cameraSettings.rotationSmoothSpeed * Time.deltaTime
            );
        }

        public void SetDistance(float distance)
        {
            _runtimeData.TargetDistance = Mathf.Clamp(distance, _cameraSettings.minDistance, _cameraSettings.maxDistance);
        }

        public void SetHeight(float height)
        {
            _runtimeData.TargetHeight = Mathf.Clamp(height, _cameraSettings.minHeight, _cameraSettings.maxHeight);
        }

        public void ResetToDefault()
        {
            _runtimeData?.ResetToDefault(_cameraSettings);
        }

        // Método para obtener la dirección forward de la cámara (útil para el movimiento del jugador)
        public Vector3 GetForwardDirection()
        {
            Vector3 forward = transform.forward;
            forward.y = 0;
            return forward.normalized;
        }

        public Vector3 GetRightDirection()
        {
            Vector3 right = transform.right;
            right.y = 0;
            return right.normalized;
        }

        private void OnDrawGizmos()
        {
            if (_target == null || !Application.isPlaying) return;

            // Dibujar el pivot
            if (_cameraPivot != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_cameraPivot.position, 0.2f);
            }

            // Dibujar la línea de colisión
            Gizmos.color = Color.red;
            if (_cameraPivot != null)
            {
                Gizmos.DrawLine(_cameraPivot.position, transform.position);
            }

            // Dibujar el radio de colisión
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _cameraSettings?.collisionRadius ?? 0.3f);

            // NUEVO: Mostrar estado actual
            Gizmos.color = Color.cyan;
            if (_currentState != null)
            {
                // Dibujar nombre del estado (solo visible en Scene view)
#if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    transform.position + Vector3.up * 0.5f,
                    $"State: {CurrentStateName}"
                );
#endif
            }

        }
    }
}