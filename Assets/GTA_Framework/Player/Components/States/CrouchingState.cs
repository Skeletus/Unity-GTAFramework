using UnityEngine;

namespace GTAFramework.Player.Components.States
{
    /// <summary>
    /// State when the player is crouching
    /// </summary>
    public class CrouchingState : PlayerState
    {
        public CrouchingState(PlayerController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            _controller.IsCrouching = true;
            _controller.IsSprinting = false; // Can't sprint while crouching
        }

        public override void Update()
        {
            // Crouching movement is handled by PlayerMovementSystem
        }

        public override PlayerState CheckTransitions()
        {
            // Check if we're in the air (can happen if crouching off a ledge)
            if (!_controller.IsGroundedStable)
            {
                return _controller.AirborneState;
            }

            // Check if no longer crouching
            if (!_controller.IsCrouching)
            {
                float horizontalSpeed = new Vector3(_controller.Velocity.x, 0, _controller.Velocity.z).magnitude;

                if (horizontalSpeed < 0.1f)
                {
                    return _controller.IdleState;
                }
                else
                {
                    return _controller.WalkingState;
                }
            }

            // Stay in crouching
            return null;
        }
    }
}