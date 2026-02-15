using UnityEngine;
using GTAFramework.Core.Interfaces;
using GTAFramework.Core.Services;
using GTAFramework.Core.Container;
using GTAFramework.Vehicle.Components;
using GTAFramework.Vehicle.Interfaces;
using GTAFramework.Player.Components;
using GTAFramework.Vehicle.Commands;

namespace GTAFramework.Vehicle.Systems
{
    [AutoRegister(Priority = 15, StartActive = true)]
    public class VehicleSystem : IGameSystem
    {
        public bool IsActive { get; set; } = true;

        [Inject] private InputService _inputService;

        // Configuración
        private readonly float _interactionRange = 3f;

        // Componentes delegados
        private VehicleRegistry _registry;
        private VehicleEnterExitHandler _enterExitHandler;
        private VehicleCommandExecutor _commandExecutor;

        // Referencia al conductor actual
        private IDriver _driver;

        public bool IsPlayerInVehicle => _enterExitHandler?.IsInVehicle ?? false;

        public void Initialize()
        {
            _inputService = DIContainer.Instance.Resolve<InputService>();

            // Crear componentes
            _registry = new VehicleRegistry();
            _enterExitHandler = new VehicleEnterExitHandler(_registry, _interactionRange);
            _commandExecutor = new VehicleCommandExecutor(_inputService);

            // Registrar vehículos existentes en la escena
            var vehicles = Object.FindObjectsByType<VehicleController>(FindObjectsSortMode.None);
            foreach (var v in vehicles)
            {
                _registry.Register(v);
            }

            var playerController = Object.FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                SetDriver(playerController);
                Debug.Log($"[VehicleSystem] Player found: {playerController.name}");
            }
            else
            {
                Debug.LogWarning("[VehicleSystem] No PlayerController found in scene!");
            }


            Debug.Log($"[VehicleSystem] Initialized with {vehicles.Length} vehicles registered.");
        }

        public void SetDriver(IDriver driver)
        {
            _driver = driver;
            _enterExitHandler?.SetDriver(driver);
        }

        public void Tick(float deltaTime)
        {
            if (_driver == null) return;

            if (_inputService.IsInteractPressed)
            {
                _inputService.IsInteractPressed = false;
                HandleInteraction();
            }

            // Ejecutar comandos si está en vehículo
            if (IsPlayerInVehicle)
            {
                _commandExecutor?.ExecuteCommands(deltaTime);
            }
        }

        private void HandleInteraction()
        {
            if (IsPlayerInVehicle)
            {
                ExitVehicle();
            }
            else
            {
                TryEnterVehicle();
            }
        }

        private void TryEnterVehicle()
        {
            if (_enterExitHandler.TryEnterNearest())
            {
                _commandExecutor.SetVehicle(_enterExitHandler.CurrentVehicle);
            }
        }

        private void ExitVehicle()
        {
            _enterExitHandler.Exit();
            _commandExecutor.Clear();
        }

        // ========== VEHICLE REGISTRATION (para vehículos que se instancian dinámicamente) ==========

        public void RegisterVehicle(VehicleController vehicle) => _registry.Register(vehicle);
        public void UnregisterVehicle(VehicleController vehicle) => _registry.Unregister(vehicle);

        public void LateTick(float deltaTime) { }
        public void FixedTick(float fixedDeltaTime) { }
        public void Shutdown() { }
    }
}