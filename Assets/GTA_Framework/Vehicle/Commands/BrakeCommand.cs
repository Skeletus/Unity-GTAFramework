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

            float forwardSpeed = Vector3.Dot(_controller.Rigidbody.linearVelocity, _controller.Transform.forward);
            bool isMovingForward = forwardSpeed > _controller.Data.SPEED_THRESHOLD;

            // Solo aplicar freno si:
            // 1. Se presiona S (input.y negativo)
            // 2. El vehículo está moviéndose hacia adelante
            float brakeInput = 0f;
            if (_input.MovementInput.y < 0f && isMovingForward)
            {
                brakeInput = Mathf.Abs(_input.MovementInput.y);
            }

            _controller.Physics.BrakeInput = brakeInput;
            _controller.Physics.Handbrake = _input.IsSprintPressed;
        }
    }
}