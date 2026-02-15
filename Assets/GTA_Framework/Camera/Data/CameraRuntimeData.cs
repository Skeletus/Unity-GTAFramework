using System;
using UnityEngine;

namespace GTAFramework.GTACamera.Data
{
    /// <summary>
    /// Datos de runtime de la cámara que se modifican durante el juego.
    /// Separado de CameraSettingsData (configuración estática).
    /// </summary>
    [Serializable]
    public class CameraRuntimeData
    {
        public float CurrentDistance;
        public float CurrentHeight;
        public float TargetDistance;
        public float TargetHeight;
        public float CurrentYaw;
        public float CurrentPitch;

        public void ResetToDefault(CameraSettingsData settings)
        {
            CurrentDistance = settings.normalDistance;
            CurrentHeight = settings.normalHeight;
            TargetDistance = settings.normalDistance;
            TargetHeight = settings.normalHeight;
            CurrentYaw = 0f;
            CurrentPitch = 0f;
        }
    }
}
