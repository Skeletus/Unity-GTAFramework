using GTAFramework.Vehicle.Components;
using GTAFramework.Core.Services;

namespace GTAFramework.Vehicle.Commands
{
    /// <summary>
    /// Comando que maneja la dirección del vehículo.
    /// </summary>
    public class SteerCommand : IVehicleCommand
    {
        private readonly VehicleController _controller;
        private readonly InputService _input;

        public string CommandName => "Steer";

        public SteerCommand(VehicleController controller, InputService inputService)
        {
            _controller = controller;
            _input = inputService;
        }

        public void Execute(float deltaTime)
        {
            if (_controller?.Physics == null || _input == null) return;

            // Input horizontal: A/D o flechas izquierda/derecha
            _controller.Physics.SteerInput = _input.MovementInput.x;
        }
    }
}