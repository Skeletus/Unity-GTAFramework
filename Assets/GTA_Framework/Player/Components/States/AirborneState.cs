using UnityEngine;

namespace GTAFramework.Player.Components.States
{
    /// <summary>
    /// State when the player is in the air (jumping or falling)
    /// </summary>
    public class AirborneState : PlayerState
    {
        public AirborneState(PlayerController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            // Player is now in the air
            // IsSprinting state is maintained for landing transition
        }

        public override void Update()
        {
            // Air movement is handled by PlayerMovementSystem
            // Gravity is applied in PlayerMovementSystem.HandleGravity()
        }

        public override PlayerState CheckTransitions()
        {
            // Check if we've landed
            if (_controller.IsGroundedStable)
            {
                // Determine which ground state to transition to
                if (_controller.IsCrouching)
                {
                    return _controller.CrouchingState;
                }

                float horizontalSpeed = new Vector3(_controller.Velocity.x, 0, _controller.Velocity.z).magnitude;

                if (horizontalSpeed < 0.1f)
                {
                    return _controller.IdleState;
                }
                else if (_controller.IsSprinting && horizontalSpeed > _controller.MovementData.runSpeed + 0.5f)
                {
                    return _controller.RunningState;
                }
                else
                {
                    return _controller.WalkingState;
                }
            }

            // Stay in airborne
            return null;
        }
    }
}