using UnityEngine;

namespace GTAFramework.Player.Data
{
    [CreateAssetMenu(fileName = "PlayerMovementData", menuName = "GTA Framework/Player/Movement Data")]
    public class PlayerMovementData : ScriptableObject
    {
        [Header("Movement Speeds")]
        [Tooltip("Slow Walking speed")]
        public float slowWalkingSpeed = 1.7f;

        [Tooltip("Normal walking speed")]
        public float walkSpeed = 3.5f;

        [Tooltip("Running speed")]
        public float runSpeed = 6f;

        [Tooltip("Sprinting speed")]
        public float sprintSpeed = 9f;

        [Tooltip("Crouching speed")]
        public float crouchSpeed = 2f;

        [Header("Movement Settings")]
        [Tooltip("How fast the player accelerates")]
        [Range(1f, 20f)]
        public float acceleration = 10f;

        [Tooltip("How fast the player decelerates")]
        [Range(1f, 20f)]
        public float deceleration = 10f;

        [Header("Rotation")]
        [Tooltip("How fast the player rotates to face movement direction")]
        [Range(1f, 30f)]
        public float rotationSpeed = 15f;

        [Header("Jump")]
        [Tooltip("Height of the jump")]
        public float jumpHeight = 2.5f;

        [Header("Gravity")]
        [Tooltip("Gravity applied to the player")]
        public float gravity = -15f;

        [Tooltip("Maximum fall speed")]
        public float maxFallSpeed = -20f;

        [Header("Grounding / Slopes")]
        [Tooltip("Layers considered as ground for grounding checks.")]
        public LayerMask groundMask = ~0;

        [Tooltip("Extra distance (meters) to probe below the capsule for ground. Helps with ramps/stairs.")]
        [Min(0f)] public float groundProbeDistance = 0.25f;

        [Tooltip("SphereCast radius multiplier based on CharacterController.radius.")]
        [Range(0.5f, 1.0f)] public float groundProbeRadiusFactor = 0.95f;

        [Tooltip("If we are \"not grounded\" but we are within this distance to valid ground while moving down, snap down to it.")]
        [Min(0f)] public float groundSnapDistance = 0.35f;

        [Tooltip("Constant downward speed applied while grounded to keep contact on down slopes (\"stick to ground\").")]
        [Min(0f)] public float stickToGroundForce = 5f;

        [Tooltip("Small grace time after leaving ground (seconds). Prevents false falling on small steps/ramps.")]
        [Min(0f)] public float coyoteTime = 0.08f;


        [Header("Crouch Settings")]
        [Tooltip("Height multiplier when crouched (e.g., 0.5 = half height)")]
        [Range(0.3f, 0.9f)] public float crouchHeightMultiplier = 0.5f;

        [Tooltip("How fast the character controller transitions between standing and crouching")]
        [Range(1f, 20f)] public float crouchTransitionSpeed = 10f;
 
    }
}
