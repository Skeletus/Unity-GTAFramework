using UnityEngine;

namespace GTAFramework.GTA_Animation.Data
{
    /// <summary>
    /// Cachea hashes de parámetros para evitar StringToHash en runtime.
    /// </summary>
    public readonly struct AnimatorParamIds
    {
        public readonly int Speed;
        public readonly int IsGrounded;
        public readonly int IsCrouching;
        public readonly int VerticalSpeed;

        // NUEVO: booleans de salto
        public readonly int IsJumping;
        public readonly int IsFalling;
        public readonly int IsLanding;

        public AnimatorParamIds(CharacterAnimationConfig cfg)
        {
            Speed = Animator.StringToHash(cfg.speedParam);
            IsGrounded = Animator.StringToHash(cfg.isGroundedParam);
            IsCrouching = Animator.StringToHash(cfg.isCrouchingParam);
            VerticalSpeed = Animator.StringToHash(cfg.verticalSpeedParam);

            IsJumping = Animator.StringToHash(cfg.isJumpingParam);
            IsFalling = Animator.StringToHash(cfg.isFallingParam);
            IsLanding = Animator.StringToHash(cfg.isLandingParam);
        }
    }
}
