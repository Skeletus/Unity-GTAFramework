using UnityEngine;
using GTAFramework.Core.Interfaces;
using GTAFramework.Core.Services;
using GTAFramework.Core.Container;
using GTAFramework.Vehicle.Components;
using GTAFramework.Vehicle.Interfaces;
using GTAFramework.Player.Components;

namespace GTAFramework.Vehicle.Systems
{
    [AutoRegister(Priority = 15, StartActive = true)]
    public class VehicleSystem : IGameSystem
    {
        public bool IsActive { get; set; } = true;

        [Inject] private InputService _inputService;

        private VehicleController _currentVehicle;
        private IDriver _player;

        public bool IsPlayerInVehicle => _currentVehicle != null;

        public void Initialize()
        {
            _inputService = DIContainer.Instance.Resolve<InputService>();
            Debug.Log("VehicleSystem initialized.");

            // Buscar automáticamente el jugador
            var playerController = Object.FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                _player = playerController;
                Debug.Log($"VehicleSystem initialized. Player found: {playerController.name}");
            }
            else
            {
                Debug.LogWarning("VehicleSystem: No PlayerController found in scene!");
            }
        }

        public void Tick(float deltaTime)
        {
            if (_inputService.IsInteractPressed) // Nueva acción
            {
                _inputService.IsInteractPressed = false;

                if (IsPlayerInVehicle)
                    ExitVehicle();
                else
                    TryEnterNearestVehicle();
            }
        }

        public void SetPlayer(IDriver player) => _player = player;

        private void TryEnterNearestVehicle()
        {
            var vehicles = Object.FindObjectsByType<VehicleController>(FindObjectsSortMode.None);
            VehicleController nearest = null;
            float nearestDist = 3f; // Rango máximo

            foreach (var v in vehicles)
            {
                if (v.IsOccupied) continue;
                float dist = Vector3.Distance(_player.Transform.position, v.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = v;
                }
            }

            if (nearest != null)
                EnterVehicle(nearest);
        }

        private void EnterVehicle(VehicleController vehicle)
        {
            _currentVehicle = vehicle;
            vehicle.Enter(_player);
        }

        private void ExitVehicle()
        {
            _currentVehicle?.Exit();
            _currentVehicle = null;
        }

        public void LateTick(float deltaTime) { }
        public void FixedTick(float fixedDeltaTime) { }
        public void Shutdown() { }
    }
}