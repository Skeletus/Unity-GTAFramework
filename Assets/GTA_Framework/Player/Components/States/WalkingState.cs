using UnityEngine;

namespace GTAFramework.Player.Components.States
{
    /// <summary>
    /// State when the player is walking (normal or slow walk)
    /// </summary>
    public class WalkingState : PlayerState
    {
        public WalkingState(PlayerController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            // Walking state is set by PlayerMovementSystem
        }

        public override void Update()
        {
            // Movement is handled by PlayerMovementSystem
        }

        public override PlayerState CheckTransitions()
        {
            // Check if we're in the air
            if (!_controller.IsGroundedStable)
            {
                return _controller.AirborneState;
            }

            // Check if crouching
            if (_controller.IsCrouching)
            {
                return _controller.CrouchingState;
            }

            // Check if stopped moving
            float horizontalSpeed = new Vector3(_controller.Velocity.x, 0, _controller.Velocity.z).magnitude;
            if (horizontalSpeed < 0.1f)
            {
                return _controller.IdleState;
            }

            // Check if sprinting
            if (_controller.IsSprinting && horizontalSpeed > _controller.MovementData.runSpeed + 0.5f)
            {
                return _controller.RunningState;
            }

            // Stay in walking
            return null;
        }
    }
}