using GTAFramework.GTACamera.Interfaces;
using UnityEngine;

namespace GTAFramework.GTACamera.Data
{
    /// <summary>
    /// Contexto que se pasa a los estados durante la transición.
    /// Contiene todas las referencias necesarias para que el estado funcione.
    /// </summary>
    public class StateTransitionContext
    {
        public Transform Target { get; set; }
        public Transform CameraPivot { get; set; }
        public CameraSettingsData Settings { get; set; }
        public CameraRuntimeData RuntimeData { get; set; }
        public IZoomStrategy ZoomStrategy { get; set; }
        public ICameraCollisionHandler CollisionHandler { get; set; }
    }
}
