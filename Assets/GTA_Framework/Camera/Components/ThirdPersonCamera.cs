using UnityEngine;
using GTAFramework.GTACamera.Data;

namespace GTAFramework.GTACamera.Components
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CameraSettingsData _cameraSettings;
        [SerializeField] private Transform _target;
        [SerializeField] private Transform _cameraPivot; // Punto donde la cámara mira

        [Header("Runtime Info (Read Only)")]
        [SerializeField] private float _currentDistance;
        [SerializeField] private float _currentHeight;
        [SerializeField] private float _currentYaw;
        [SerializeField] private float _currentPitch;

        private UnityEngine.Camera _camera;
        private float _targetDistance;
        private float _targetHeight;
        private Vector3 _desiredPosition;
        private float _dynamicZoomOffset;

        // Propiedades públicas
        public CameraSettingsData Settings => _cameraSettings;
        public Transform Target => _target;
        public Transform CameraPivot => _cameraPivot;
        public float CurrentYaw => _currentYaw;
        public float CurrentPitch => _currentPitch;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();

            if (_camera == null)
            {
                Debug.LogError("ThirdPersonCamera: Camera component not found!");
            }

            // Inicializar valores
            if (_cameraSettings != null)
            {
                _currentDistance = _cameraSettings.normalDistance;
                _currentHeight = _cameraSettings.normalHeight;
                _targetDistance = _currentDistance;
                _targetHeight = _currentHeight;
            }

            // Crear pivot si no existe
            if (_cameraPivot == null && _target != null)
            {
                GameObject pivotObj = new GameObject("CameraPivot");
                _cameraPivot = pivotObj.transform;
                _cameraPivot.SetParent(_target);
                _cameraPivot.localPosition = Vector3.up * _cameraSettings.pivotYOffset;
            }
        }

        public void SetTarget(Transform target)
        {
            _target = target;

            // Reposicionar pivot
            if (_cameraPivot != null && _target != null)
            {
                _cameraPivot.SetParent(_target);
                _cameraPivot.localPosition = Vector3.up * _cameraSettings.pivotYOffset;
            }
        }

        public void RotateCamera(Vector2 lookInput)
        {
            if (_cameraSettings == null) return;

            // Rotar horizontalmente (yaw)
            _currentYaw += lookInput.x * _cameraSettings.horizontalSensitivity;

            // Rotar verticalmente (pitch)
            _currentPitch -= lookInput.y * _cameraSettings.verticalSensitivity;
            _currentPitch = Mathf.Clamp(_currentPitch, _cameraSettings.minVerticalAngle, _cameraSettings.maxVerticalAngle);

            // Calcular el zoom dinámico estilo GTA
            CalculateDynamicZoom();
        }

        public void UpdateCameraPosition()
        {
            if (_target == null || _cameraPivot == null || _cameraSettings == null) return;

            // Calcular la posición deseada
            Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0);

            // Aplicar el zoom dinámico basado en el ángulo vertical
            float effectiveDistance = _targetDistance + _dynamicZoomOffset;

            Vector3 offset = rotation * new Vector3(0, _targetHeight, -effectiveDistance);
            _desiredPosition = _cameraPivot.position + offset;

            // Detectar colisiones
            Vector3 finalPosition = HandleCameraCollision(_cameraPivot.position, _desiredPosition);

            // Suavizar el movimiento
            transform.position = Vector3.Lerp(
                transform.position,
                finalPosition,
                _cameraSettings.positionSmoothSpeed * Time.deltaTime
            );

            // Actualizar la rotación para mirar al pivot
            Quaternion targetRotation = Quaternion.LookRotation(_cameraPivot.position - transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                _cameraSettings.rotationSmoothSpeed * Time.deltaTime
            );

            // Actualizar valores actuales suavemente
            _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, Time.deltaTime * 5f);
            _currentHeight = Mathf.Lerp(_currentHeight, _targetHeight, Time.deltaTime * 5f);
        }

        private void CalculateDynamicZoom()
        {
            // Estilo GTA: cuando miras hacia abajo, la cámara se acerca
            if (_currentPitch > _cameraSettings.downwardZoomStartAngle)
            {
                float normalizedAngle = (_currentPitch - _cameraSettings.downwardZoomStartAngle) /
                                       (_cameraSettings.maxVerticalAngle - _cameraSettings.downwardZoomStartAngle);

                float maxZoom = _targetDistance * _cameraSettings.downwardZoomFactor;
                float targetZoom = -Mathf.Lerp(0, maxZoom, normalizedAngle);

                _dynamicZoomOffset = Mathf.Lerp(
                    _dynamicZoomOffset,
                    targetZoom,
                    _cameraSettings.zoomTransitionSpeed * Time.deltaTime
                );
            }
            else
            {
                _dynamicZoomOffset = Mathf.Lerp(
                    _dynamicZoomOffset,
                    0,
                    _cameraSettings.zoomTransitionSpeed * Time.deltaTime
                );
            }
        }

        private Vector3 HandleCameraCollision(Vector3 from, Vector3 to)
        {
            Vector3 direction = to - from;
            float distance = direction.magnitude;

            if (Physics.SphereCast(
                from,
                _cameraSettings.collisionRadius,
                direction.normalized,
                out RaycastHit hit,
                distance,
                _cameraSettings.collisionLayers,
                QueryTriggerInteraction.Ignore))
            {
                // Ajustar la posición para evitar el clipping
                float adjustedDistance = hit.distance - _cameraSettings.collisionOffset;
                adjustedDistance = Mathf.Max(adjustedDistance, _cameraSettings.minDistance);

                return from + direction.normalized * adjustedDistance;
            }

            return to;
        }

        public void SetDistance(float distance)
        {
            _targetDistance = Mathf.Clamp(distance, _cameraSettings.minDistance, _cameraSettings.maxDistance);
        }

        public void SetHeight(float height)
        {
            _targetHeight = Mathf.Clamp(height, _cameraSettings.minHeight, _cameraSettings.maxHeight);
        }

        public void ResetToDefault()
        {
            if (_cameraSettings != null)
            {
                _targetDistance = _cameraSettings.normalDistance;
                _targetHeight = _cameraSettings.normalHeight;
            }
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
        }
    }
}