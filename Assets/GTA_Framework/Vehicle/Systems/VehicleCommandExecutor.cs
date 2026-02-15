using GTAFramework.Core.Services;
using GTAFramework.Vehicle.Components;
using GTAFramework.Vehicle.Commands;

namespace GTAFramework.Vehicle.Systems
{
    /// <summary>
    /// Ejecuta comandos de conducción.
    /// Reutiliza instancias de comandos en lugar de recrearlos.
    /// </summary>
    public class VehicleCommandExecutor
    {
        private readonly InputService _inputService;

        // Comandos reutilizados
        private AccelerateCommand _accelerateCommand;
        private SteerCommand _steerCommand;
        private BrakeCommand _brakeCommand;

        private VehicleController _vehicle;

        public VehicleCommandExecutor(InputService inputService)
        {
            _inputService = inputService;
        }

        public void SetVehicle(VehicleController vehicle)
        {
            if (_vehicle == vehicle) return;

            _vehicle = vehicle;

            if (vehicle != null)
            {
                // Crear comandos solo cuando cambia el vehículo
                _accelerateCommand = new AccelerateCommand(vehicle, _inputService);
                _steerCommand = new SteerCommand(vehicle, _inputService);
                _brakeCommand = new BrakeCommand(vehicle, _inputService);
            }
            else
            {
                _accelerateCommand = null;
                _steerCommand = null;
                _brakeCommand = null;
            }
        }

        public void ExecuteCommands(float deltaTime)
        {
            if (_vehicle == null) return;

            _accelerateCommand?.Execute(deltaTime);
            _steerCommand?.Execute(deltaTime);
            _brakeCommand?.Execute(deltaTime);
        }

        public void Clear()
        {
            _vehicle = null;
            _accelerateCommand = null;
            _steerCommand = null;
            _brakeCommand = null;
        }
    }
}