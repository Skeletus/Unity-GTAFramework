using UnityEngine;
using GTAFramework.Player.Components;

namespace GTAFramework.Player.Commands
{
    /// <summary>
    /// Comando que maneja salto + gravedad (vertical).
    /// Replica la lógica de HandleGravity del sistema original.
    /// </summary>
    public class JumpCommand : IPlayerCommand
    {
        private readonly PlayerController _controller;

        private float _verticalVelocity;
        private bool _jumpRequested;

        public string CommandName => "Jump";

        /// <summary>Velocidad vertical actual.</summary>
        public float VerticalVelocity => _verticalVelocity;

        public JumpCommand(PlayerController controller)
        {
            _controller = controller;
        }

        /// <summary>Solicita un salto (se ejecutará si CanJump).</summary>
        public void RequestJump()
        {
            _jumpRequested = true;
        }

        public void Execute(float deltaTime)
        {
            if (_controller == null || _controller.MovementData == null)
                return;

            var data = _controller.MovementData;

            // Jump
            if (_jumpRequested && _controller.CanJump)
            {
                _verticalVelocity = Mathf.Sqrt(data.jumpHeight * -2f * data.gravity);
                _jumpRequested = false;

                _controller.NotifyJump(_verticalVelocity);
            }

            // Stick to ground on ramps/stairs
            if (_controller.IsGroundedContact && _verticalVelocity <= 0f)
            {
                _verticalVelocity = -Mathf.Max(2f, data.stickToGroundForce);
                return;
            }

            // Gravity in air
            _verticalVelocity += data.gravity * deltaTime;
            _verticalVelocity = Mathf.Max(_verticalVelocity, data.maxFallSpeed);
        }

        public void ResetVerticalVelocity()
        {
            _verticalVelocity = 0f;
            _jumpRequested = false;
        }
    }
}
