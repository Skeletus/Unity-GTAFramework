using UnityEngine;
using GTAFramework.GTA_Animation.Components;
using GTAFramework.GTA_Animation.Data;

namespace GTAFramework.GTA_Animation.Modules
{
    internal sealed class AirborneModule : IAnimationModule
    {
        private CharacterAnimationAgent _agent;

        private bool _wasGrounded;

        // “hold” de IsJumping (bool) para entrar a JumpStart sin trigger
        private float _jumpHoldUntilTime;

        public void Initialize(CharacterAnimationAgent agent)
        {
            _agent = agent;
            _wasGrounded = true;
            _jumpHoldUntilTime = 0f;
        }

        public void Tick(float dt, ref AnimationBlackboard bb, AnimatorDriver driver)
        {
            var cfg = _agent.Config;

            // Base params
            driver.SetBool(driver.Ids.IsGrounded, bb.isGrounded);
            driver.SetBool(driver.Ids.IsLanding, bb.isMovementLocked); // landing lock = no move/no jump

            // Si aterrizamos (air -> ground), se termina el hold de salto
            if (bb.isGrounded && !_wasGrounded)
            {
                _jumpHoldUntilTime = 0f;
            }

            // Registrar request de salto (one-frame desde gameplay)
            if (bb.jumpPressedThisFrame && !bb.isMovementLocked)
            {
                // Mantener IsJumping true una ventana corta
                _jumpHoldUntilTime = Time.time + cfg.jumpStartHoldTime;

                if (cfg.logWarnings)
                {
                    Debug.Log($"[AirborneModule] Jump request => holding IsJumping for {cfg.jumpStartHoldTime:F3}s (Agent={_agent.name})");
                }
            }

            bool canCoyoteJump = !bb.isGrounded && bb.timeSinceGrounded <= cfg.coyoteTime;
            bool canStartJumpAnim = (bb.isGrounded || canCoyoteJump) && !bb.isMovementLocked;

            bool isJumping = canStartJumpAnim && Time.time <= _jumpHoldUntilTime;

            bool isFalling =
                !bb.isGrounded &&
                bb.timeInAir >= cfg.minAirTimeForFall &&
                bb.verticalSpeed < cfg.fallingVerticalSpeedThreshold;

            // Publicar booleans
            driver.SetBool(driver.Ids.IsJumping, isJumping);
            driver.SetBool(driver.Ids.IsFalling, isFalling);

            _wasGrounded = bb.isGrounded;
        }

        public void LateTick(float dt, ref AnimationBlackboard bb, AnimatorDriver driver) { }
    }
}
