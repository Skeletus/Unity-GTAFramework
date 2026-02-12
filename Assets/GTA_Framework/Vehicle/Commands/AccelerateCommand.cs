using GTAFramework.Vehicle.Components;
using GTAFramework.Core.Services;

namespace GTAFramework.Vehicle.Commands
{
    /// <summary>
    /// Comando que maneja la aceleración y reversa del vehículo.
    /// </summary>
    public class AccelerateCommand : IVehicleCommand
    {
        private readonly VehicleController _controller;
        private readonly InputService _input;

        public string CommandName => "Accelerate";

        public AccelerateCommand(VehicleController controller, InputService inputService)
        {
            _controller = controller;
            _input = inputService;
        }

        public void Execute(float deltaTime)
        {
            if (_controller?.Physics == null || _input == null) return;

            // Input vertical: W/S o flechas arriba/abajo
            // Valor positivo = acelerar, negativo = reversa
            _controller.Physics.MotorInput = _input.MovementInput.y;
        }
    }
}