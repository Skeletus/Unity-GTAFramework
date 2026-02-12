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

        public void SetCameraTarget(Transform target)
        {
            if (_thirdPersonCamera != null)
            {
                _thirdPersonCamera.SetTarget(target);
            }
        }

        public Components.ThirdPersonCamera GetCamera()
        {
            return _thirdPersonCamera;
        }
    }
}