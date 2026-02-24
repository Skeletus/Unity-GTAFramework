using UnityEngine;

namespace GTAFramework.GTA_Animation.Data
{
    [System.Serializable]
    public struct AnimationBlackboard
    {
        public float deltaTime;

        public Vector3 velocity;
        public float verticalSpeed;

        public bool isGrounded;
        public bool isCrouching;

        // NUEVO: �til para landing lock
        public bool isMovementLocked;

        // Input one-frame
        public bool jumpPressedThisFrame;

        public float timeSinceGrounded;
        public float timeInAir;

        public bool isAiming;
        public float moveX;
        public float moveZ;

        public void ResetOneFrameFlags()
        {
            jumpPressedThisFrame = false;
        }
    }
}
