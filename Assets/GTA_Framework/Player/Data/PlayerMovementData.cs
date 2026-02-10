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
    }
}