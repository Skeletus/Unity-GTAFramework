using GTAFramework.GTACamera.Data;
using GTAFramework.GTACamera.Interfaces;
using UnityEngine;

namespace GTAFramework.GTACamera.Strategies.Zoom
{
    /// <summary>
    /// Estrategia sin zoom - usada en estados cinemáticos donde el zoom
    /// automático no es deseado.
    /// </summary>
    public class NoZoomStrategy : IZoomStrategy
    {
        public float CalculateZoomOffset(float pitch, float targetDistance, CameraSettingsData settings)
        {
            return 0f; // Sin zoom
        }

        public void Reset() { }
    }
}
