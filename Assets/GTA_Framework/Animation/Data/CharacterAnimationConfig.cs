using UnityEngine;

namespace GTAFramework.GTA_Animation.Data
{
    [CreateAssetMenu(
        fileName = "CharacterAnimationConfig",
        menuName = "GTA Framework/Animation/Character Animation Config",
        order = 10)]
    public class CharacterAnimationConfig : ScriptableObject
    {
        [Header("Animator Parameters (Names)")]
        public string speedParam = "Speed";
        public string isGroundedParam = "IsGrounded";
        public string isCrouchingParam = "IsCrouching";
        public string verticalSpeedParam = "VerticalSpeed";

        // NUEVO: booleans para salto
        public string isJumpingParam = "IsJumping";
        public string isFallingParam = "IsFalling";
        public string isLandingParam = "IsLanding";

        [Header("Tuning")]
        public LocomotionTuning locomotion = new LocomotionTuning();

        [Header("Airborne")]
        [Tooltip("Ventana para permitir saltar justo después de dejar el suelo (coyote time).")]
        [Min(0f)] public float coyoteTime = 0.12f;

        [Tooltip("Tiempo mínimo en el aire para considerar que ya estás realmente airborne (evita false positives).")]
        [Min(0f)] public float minAirTimeForFall = 0.08f;

        [Tooltip("Ventana (segundos) que mantiene IsJumping=true tras presionar jump (para entrar a JumpStart sin Trigger).")]
        [Min(0f)] public float jumpStartHoldTime = 0.10f;

        [Tooltip("Umbral para considerar caída. Ej: -0.1 o -0.2 (más negativo = más estricto).")]
        public float fallingVerticalSpeedThreshold = -0.1f;

        [Header("Debug")]
        public bool logWarnings = true;
    }
}
