using UnityEngine;
using GTAFramework.Player.Components;
using GTAFramework.Core.Services;

namespace GTAFramework.Player.Commands
{
    /// <summary>
    /// Comando que rota al jugador hacia la dirección de movimiento.
    /// Si está apuntando, rota hacia la dirección forward de la cámara.
    /// </summary>
    public class RotateCommand : IPlayerCommand
    {
        private readonly PlayerController _controller;
        private readonly InputService _input;

        public string CommandName => "Rotate";

        public RotateCommand(PlayerController controller, InputService inputService)
        {
            _controller = controller;
            _input = inputService;
        }

        public void Execute(float deltaTime)
        {
            if (_controller == null || _controller.MovementData == null || _input == null)
                return;

            if (_input.IsAiming)
            {
                RotateTowardsCamera(deltaTime);
                return;
            }

            Vector2 input = _input.MovementInput;

            if (input.magnitude <= 0.1f)
                return;

            Vector3 moveDirection = PlayerMovementUtils.GetMovementDirection(_controller, input);

            if (moveDirection.magnitude <= 0.1f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            _controller.transform.rotation = Quaternion.Slerp(
                _controller.transform.rotation,
                targetRotation,
                _controller.MovementData.rotationSpeed * deltaTime
            );
        }

        private void RotateTowardsCamera(float deltaTime)
        {
            Vector3 forward = _controller.transform.forward;

            if (_controller.CameraTransform != null)
                forward = _controller.CameraTransform.forward;

            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(forward.normalized);
            _controller.transform.rotation = Quaternion.Slerp(
                _controller.transform.rotation,
                targetRotation,
                _controller.MovementData.rotationSpeed * deltaTime
            );
        }
    }
}
