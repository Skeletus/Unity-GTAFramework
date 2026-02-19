using UnityEngine;

namespace GTAFramework.Player.Components.States
{
    /// <summary>
    /// State when the player is standing still on the ground
    /// </summary>
    public class IdleState : PlayerState
    {
        public IdleState(PlayerController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            _controller.IsWalking = false;
            _controller.IsSprinting = false;
        }

        public override void Update()
        {
            // Idle state doesn't need to do anything special each frame
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

            // Determine if walking or running based on speed
            float horizontalSpeed = new Vector3(_controller.Velocity.x, 0, _controller.Velocity.z).magnitude;

            // Check if moving
            if (horizontalSpeed > 0.5f)
            {
                

                if (_controller.IsSprinting && horizontalSpeed > _controller.MovementData.runSpeed + 0.5f)
                {
                    return _controller.RunningState;
                }
                else
                {
                    return _controller.WalkingState;
                }
            }

            // Stay in idle
            return null;
        }
    }
}