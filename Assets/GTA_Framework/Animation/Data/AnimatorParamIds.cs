using UnityEngine;

namespace GTAFramework.GTA_Animation.Data
{
    /// <summary>
    /// Cachea hashes de parametros para evitar StringToHash en runtime.
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

        // NUEVO: idle pistol
        public readonly int IsPistolEquipped;

        public readonly int IsAiming;
        public readonly int MoveX;
        public readonly int MoveZ;

        public AnimatorParamIds(CharacterAnimationConfig cfg)
        {
            Speed = Animator.StringToHash(cfg.speedParam);
            IsGrounded = Animator.StringToHash(cfg.isGroundedParam);
            IsCrouching = Animator.StringToHash(cfg.isCrouchingParam);
            VerticalSpeed = Animator.StringToHash(cfg.verticalSpeedParam);

            IsJumping = Animator.StringToHash(cfg.isJumpingParam);
            IsFalling = Animator.StringToHash(cfg.isFallingParam);
            IsLanding = Animator.StringToHash(cfg.isLandingParam);

            IsPistolEquipped = Animator.StringToHash(cfg.isPistolEquippedParam);

            IsAiming = Animator.StringToHash(cfg.isAimingParam);
            MoveX = Animator.StringToHash(cfg.moveXParam);
            MoveZ = Animator.StringToHash(cfg.moveZParam);
        }
    }
}
