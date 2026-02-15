using GTAFramework.GTACamera.Data;
using UnityEngine;

namespace GTAFramework.GTACamera.Interfaces
{
    /// <summary>
    /// Estrategia para calcular el zoom dinámico de la cámara.
    /// Permite diferentes comportamientos (downward zoom, no zoom, smooth zoom, etc.)
    /// </summary>
    public interface IZoomStrategy
    {
        /// <summary>
        /// Calcula el offset de zoom basado en el ángulo pitch.
        /// </summary>
        /// <param name="pitch">Ángulo vertical actual de la cámara</param>
        /// <param name="targetDistance">Distancia objetivo de la cámara</param>
        /// <param name="settings">Configuración de la cámara</param>
        /// <returns>Offset de zoom (negativo = más cerca)</returns>
        float CalculateZoomOffset(float pitch, float targetDistance, CameraSettingsData settings);

        /// <summary>
        /// Reinicia el estado interno del zoom.
        /// </summary>
        void Reset();
    }
}