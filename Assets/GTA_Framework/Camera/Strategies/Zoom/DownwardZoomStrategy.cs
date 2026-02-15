using GTAFramework.GTACamera.Data;
using GTAFramework.GTACamera.Interfaces;
using UnityEngine;

namespace GTAFramework.GTACamera.Strategies.Zoom
{
    /// <summary>
    /// Implementación de zoom estilo GTA: cuando miras hacia abajo, la cámara se acerca.
    /// Extraído de ThirdPersonCamera.CalculateDynamicZoom() líneas 123-148
    /// </summary>
    public class DownwardZoomStrategy : IZoomStrategy
    {
        // Estado interno para suavizar el zoom
        private float _currentZoomOffset;

        public float CalculateZoomOffset(float pitch, float targetDistance, CameraSettingsData settings)
        {
            // Código extraído de ThirdPersonCamera.cs líneas 126-147

            // Estilo GTA: cuando miras hacia abajo, la cámara se acerca
            if (pitch > settings.downwardZoomStartAngle)
            {
                float normalizedAngle = (pitch - settings.downwardZoomStartAngle) /
                                       (settings.maxVerticalAngle - settings.downwardZoomStartAngle);

                float maxZoom = targetDistance * settings.downwardZoomFactor;
                float targetZoom = -Mathf.Lerp(0, maxZoom, normalizedAngle);

                _currentZoomOffset = Mathf.Lerp(
                    _currentZoomOffset,
                    targetZoom,
                    settings.zoomTransitionSpeed * Time.deltaTime
                );
            }
            else
            {
                _currentZoomOffset = Mathf.Lerp(
                    _currentZoomOffset,
                    0,
                    settings.zoomTransitionSpeed * Time.deltaTime
                );
            }

            return _currentZoomOffset;
        }

        public void Reset()
        {
            _currentZoomOffset = 0f;
        }
    }
}