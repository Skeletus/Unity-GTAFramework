using UnityEngine;

namespace GTAFramework.Player.Components.States
{
    /// <summary>
    /// State when the player is running/sprinting
    /// </summary>
    public class RunningState : PlayerState
    {
        public RunningState(PlayerController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            _controller.IsSprinting = true;
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

            float horizontalSpeed = new Vector3(_controller.Velocity.x, 0, _controller.Velocity.z).magnitude;

            // Check if stopped sprinting
            if (!_controller.IsSprinting || horizontalSpeed <= _controller.MovementData.runSpeed + 0.5f)
            {
                if (horizontalSpeed < 0.1f)
                {
                    return _controller.IdleState;
                }
                else
                {
                    return _controller.WalkingState;
                }
            }

            // Stay in running
            return null;
        }
    }
}