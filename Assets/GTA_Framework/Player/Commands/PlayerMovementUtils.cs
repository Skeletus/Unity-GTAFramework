using UnityEngine;
using GTAFramework.Player.Components;

namespace GTAFramework.Player.Commands
{
    /// <summary>
    /// Helpers de movimiento compartidos entre comandos.
    /// </summary>
    public static class PlayerMovementUtils
    {
        public static Vector3 GetMovementDirection(PlayerController controller, Vector2 input)
        {
            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;

            if (controller != null && controller.CameraTransform != null)
            {
                forward = controller.CameraTransform.forward;
                right = controller.CameraTransform.right;
            }

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return (forward * input.y + right * input.x).normalized;
        }
    }
}
