using UnityEngine;
using GTAFramework.Core.Interfaces;
using GTAFramework.Core.Services;
using GTAFramework.GTACamera.Components;
using GTAFramework.Core.Container;

namespace GTAFramework.GTACamera.Systems
{
    [AutoRegister(Priority = 30, StartActive = true)]
    public class CameraSystem : IGameSystem
    {
        public bool IsActive { get; set; } = true;

        [Inject] private InputService _inputService;
        private Components.ThirdPersonCamera _thirdPersonCamera;

        // Propiedades públicas
        public ThirdPersonCamera Camera => _thirdPersonCamera;
        public string CurrentState => _thirdPersonCamera?.CurrentStateName ?? "None";

        public void Initialize()
        {
            _inputService = DIContainer.Instance.Resolve<InputService>();

            _thirdPersonCamera = Object.FindFirstObjectByType<ThirdPersonCamera>();

            if (_thirdPersonCamera == null)
            {
                Debug.LogWarning("CameraSystem: No ThirdPersonCamera found in scene!");
            }

            Debug.Log("CameraSystem initialized.");
        }

        public void Tick(float deltaTime)
        {
            // Vacío - la lógica principal está en LateTick
        }

        public void LateTick(float deltaTime)
        {
            if (_thirdPersonCamera == null || _inputService == null)
                return;

            // Obtener input del mouse
            Vector2 lookInput = _inputService.LookInput;

            // Rotar la cámara
            _thirdPersonCamera.RotateCamera(lookInput);

            // Actualizar la posición de la cámara (siempre después del movimiento del jugador)
            _thirdPersonCamera.UpdateCameraPosition();
        }

        public void FixedTick(float fixedDeltaTime)
        {
            // Vacío por ahora
        }

        public void Shutdown()
        {
            Debug.Log("CameraSystem shutdown.");
        }

        #region State Management

        /// <summary>
        /// Cambia la cámara al estado Normal.
        /// </summary>
        public void SetNormalMode()
        {
            _thirdPersonCamera?.SetState("Normal");
        }

        /// <summary>
        /// Cambia la cámara al estado Aiming (apuntado).
        /// </summary>
        public void SetAimingMode()
        {
            _thirdPersonCamera?.SetState("Aiming");
        }

        /// <summary>
        /// Cambia la cámara al estado Cinematic.
        /// </summary>
        public void SetCinematicMode()
        {
            _thirdPersonCamera?.SetState("Cinematic");
        }

        /// <summary>
        /// Cambia el estado de la cámara por nombre.
        /// </summary>
        /// <param name="stateName">"Normal", "Aiming", o "Cinematic"</param>
        public void SetCameraState(string stateName)
        {
            _thirdPersonCamera?.SetState(stateName);
        }

        #endregion

        #region Camera Configuration

        /// <summary>
        /// Ajusta la distancia de la cámara (zoom).
        /// </summary>
        public void SetCameraDistance(float distance)
        {
            _thirdPersonCamera?.SetDistance(distance);
        }

        /// <summary>
        /// Ajusta la altura de la cámara.
        /// </summary>
        public void SetCameraHeight(float height)
        {
            _thirdPersonCamera?.SetHeight(height);
        }

        /// <summary>
        /// Resetea la cámara a sus valores por defecto.
        /// </summary>
        public void ResetCameraToDefault()
        {
            _thirdPersonCamera?.ResetToDefault();
        }

        #endregion

        #region Utility

        /// <summary>
        /// Obtiene la dirección forward de la cámara (plano XZ).
        /// </summary>
        public Vector3 GetCameraForwardDirection()
        {
            return _thirdPersonCamera?.GetForwardDirection() ?? Vector3.forward;
        }

        /// <summary>
        /// Obtiene la dirección right de la cámara (plano XZ).
        /// </summary>
        public Vector3 GetCameraRightDirection()
        {
            return _thirdPersonCamera?.GetRightDirection() ?? Vector3.right;
        }

        #endregion
    }
}