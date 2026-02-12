using GTAFramework.Vehicle.Components;
using GTAFramework.Core.Services;
using UnityEngine;

namespace GTAFramework.Vehicle.Commands
{
    /// <summary>
    /// Comando que maneja el freno y freno de mano.
    /// </summary>
    public class BrakeCommand : IVehicleCommand
    {
        private readonly VehicleController _controller;
        private readonly InputService _input;

        public string CommandName => "Brake";

        public BrakeCommand(VehicleController controller, InputService inputService)
        {
            _controller = controller;
            _input = inputService;
        }

        public void Execute(float deltaTime)
        {
            if (_controller?.Physics == null || _input == null) return;

            // Freno normal: cuando se presiona S (input.y negativo)
            float brakeInput = _input.MovementInput.y < 0f ? Mathf.Abs(_input.MovementInput.y) : 0f;
            _controller.Physics.BrakeInput = brakeInput;

            // Freno de mano: puedes usar Sprint o crear una nueva acción
            // Por ahora usamos IsSprintPressed como handbrake
            _controller.Physics.Handbrake = _input.IsSprintPressed;
        }
    }
}