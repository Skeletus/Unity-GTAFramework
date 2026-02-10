using UnityEngine;

namespace GTAFramework.GTACamera.Data
{
    [CreateAssetMenu(fileName = "CameraSettingsData", menuName = "GTA Framework/Camera/Camera Settings")]
    public class CameraSettingsData : ScriptableObject
    {
        [Header("Follow Settings")]
        [Tooltip("Distancia normal de la cámara al personaje")]
        public float normalDistance = 5f;

        [Tooltip("Distancia mínima (zoom in)")]
        public float minDistance = 2f;

        [Tooltip("Distancia máxima (zoom out)")]
        public float maxDistance = 10f;

        [Header("Height Settings")]
        [Tooltip("Altura normal de la cámara sobre el personaje")]
        public float normalHeight = 2f;

        [Tooltip("Altura mínima de la cámara")]
        public float minHeight = 0.5f;

        [Tooltip("Altura máxima de la cámara")]
        public float maxHeight = 5f;

        [Header("Rotation Settings")]
        [Tooltip("Sensibilidad horizontal del mouse")]
        [Range(0.1f, 10f)]
        public float horizontalSensitivity = 3f;

        [Tooltip("Sensibilidad vertical del mouse")]
        [Range(0.1f, 10f)]
        public float verticalSensitivity = 2f;

        [Tooltip("Ángulo mínimo vertical (mirar hacia arriba)")]
        [Range(-89f, 0f)]
        public float minVerticalAngle = -20f;

        [Tooltip("Ángulo máximo vertical (mirar hacia abajo)")]
        [Range(0f, 89f)]
        public float maxVerticalAngle = 70f;

        [Header("Smoothing")]
        [Tooltip("Suavidad del seguimiento de posición")]
        [Range(1f, 30f)]
        public float positionSmoothSpeed = 10f;

        [Tooltip("Suavidad de la rotación")]
        [Range(1f, 30f)]
        public float rotationSmoothSpeed = 12f;

        [Header("Collision")]
        [Tooltip("Radio de detección de colisiones")]
        public float collisionRadius = 0.3f;

        [Tooltip("Capas con las que la cámara puede colisionar")]
        public LayerMask collisionLayers = -1;

        [Tooltip("Offset mínimo para evitar clipping")]
        public float collisionOffset = 0.2f;

        [Header("GTA Style Settings")]
        [Tooltip("Qué tan cerca se acerca la cámara cuando miras hacia abajo")]
        [Range(0f, 1f)]
        public float downwardZoomFactor = 0.6f;

        [Tooltip("Velocidad de transición del zoom al mirar abajo")]
        [Range(1f, 20f)]
        public float zoomTransitionSpeed = 8f;

        [Tooltip("Ángulo a partir del cual empieza el zoom hacia abajo")]
        [Range(30f, 70f)]
        public float downwardZoomStartAngle = 45f;

        [Header("Pivot Offset")]
        [Tooltip("Offset del pivot en Y (punto donde la cámara mira)")]
        public float pivotYOffset = 1.5f;
    }
}