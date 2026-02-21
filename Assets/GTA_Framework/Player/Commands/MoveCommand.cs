using UnityEngine;
using GTAFramework.Player.Components;
using GTAFramework.Core.Services;

namespace GTAFramework.Player.Commands
{
    /// <summary>
    /// Comando que maneja el movimiento horizontal (XZ) del jugador.
    /// Mantiene su propia velocidad horizontal (_currentVelocity) como en el sistema original.
    /// </summary>
    public class MoveCommand : IPlayerCommand
    {
        private readonly PlayerController _controller;
        private readonly InputService _input;

        private Vector3 _currentVelocity;

        public string CommandName => "Move";

        /// <summary>Velocidad horizontal calculada por el comando (sin Y).</summary>
        public Vector3 CurrentVelocity => _currentVelocity;

        public MoveCommand(PlayerController controller, InputService inputService)
        {
            _controller = controller;
            _input = inputService;
        }

        public void Execute(float deltaTime)
        {
            if (_controller == null || _controller.MovementData == null || _input == null)
                return;

            Vector2 input = _input.MovementInput;
            Vector3 moveDirection = PlayerMovementUtils.GetMovementDirection(_controller, input);
            float targetSpeed = GetTargetSpeed();

            if (moveDirection.magnitude > 0.1f)
            {
                _currentVelocity = Vector3.Lerp(
                    _currentVelocity,
                    moveDirection * targetSpeed,
                    _controller.MovementData.acceleration * deltaTime
                );
            }
            else
            {
                _currentVelocity = Vector3.Lerp(
                    _currentVelocity,
                    Vector3.zero,
                    _controller.MovementData.deceleration * deltaTime
                );
            }
        }

        private float GetTargetSpeed()
        {
            var data = _controller.MovementData;
            bool isAiming = _input.IsAiming;

            // Mantener side-effects como en el código original
            _controller.IsWalking = _input.IsWalkPressed;
            _controller.IsSprinting = !isAiming && _input.IsSprintPressed;

            if (_controller.IsWalking)
                return ApplySpeedMultiplier(data.slowWalkingSpeed);

            if (_controller.IsCrouching)
                return ApplySpeedMultiplier(data.crouchSpeed);

            if (_controller.IsSprinting)
                return ApplySpeedMultiplier(data.sprintSpeed);

            return ApplySpeedMultiplier(data.runSpeed);
        }

        private float ApplySpeedMultiplier(float baseSpeed)
        {
            float multiplier = Mathf.Clamp(_input.MovementSpeedMultiplier, 0.1f, 2f);
            return baseSpeed * multiplier;
        }

        public void ResetVelocity()
        {
            _currentVelocity = Vector3.zero;
        }
    }
}
