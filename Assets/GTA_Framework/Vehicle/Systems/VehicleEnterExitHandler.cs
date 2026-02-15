using UnityEngine;
using GTAFramework.Vehicle.Components;
using GTAFramework.Vehicle.Interfaces;

namespace GTAFramework.Vehicle.Systems
{
    /// <summary>
    /// Maneja la lógica de entrada y salida de vehículos.
    /// </summary>
    public class VehicleEnterExitHandler
    {
        private readonly VehicleRegistry _registry;
        private readonly float _interactionRange;

        private VehicleController _currentVehicle;
        private IDriver _driver;

        public VehicleController CurrentVehicle => _currentVehicle;
        public bool IsInVehicle => _currentVehicle != null;

        public VehicleEnterExitHandler(VehicleRegistry registry, float interactionRange = 3f)
        {
            _registry = registry;
            _interactionRange = interactionRange;
        }

        public void SetDriver(IDriver driver) => _driver = driver;

        public bool TryEnterNearest()
        {
            if (_driver == null) return false;

            var nearest = _registry.FindNearestAvailable(
                _driver.Transform.position,
                _interactionRange
            );

            if (nearest != null)
            {
                Enter(nearest);
                return true;
            }

            return false;
        }

        public void Exit()
        {
            if (_currentVehicle == null) return;

            _currentVehicle.Exit();
            _currentVehicle = null;
        }

        private void Enter(VehicleController vehicle)
        {
            if (vehicle.IsDestroyed) return;

            _currentVehicle = vehicle;
            vehicle.Enter(_driver);
        }
    }
}