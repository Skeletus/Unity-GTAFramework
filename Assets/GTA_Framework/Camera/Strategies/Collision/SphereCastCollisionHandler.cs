using GTAFramework.GTACamera.Interfaces;
using UnityEngine;

namespace GTAFramework.GTACamera.Strategies.Collision
{
    /// <summary>
    /// Implementación de colisión usando Physics.SphereCast.
    /// Extraído de ThirdPersonCamera.HandleCameraCollision() líneas 150-172
    /// </summary>
    public class SphereCastCollisionHandler : ICameraCollisionHandler
    {
        public Vector3 HandleCollision(
            Vector3 from,
            Vector3 to,
            float radius,
            LayerMask layers,
            float offset,
            float minDistance)
        {
            // Código extraído de ThirdPersonCamera.cs líneas 152-171
            Vector3 direction = to - from;
            float distance = direction.magnitude;

            if (Physics.SphereCast(
                from,
                radius,
                direction.normalized,
                out RaycastHit hit,
                distance,
                layers,
                QueryTriggerInteraction.Ignore))
            {
                // Ajustar la posición para evitar el clipping
                float adjustedDistance = hit.distance - offset;
                adjustedDistance = Mathf.Max(adjustedDistance, minDistance);

                return from + direction.normalized * adjustedDistance;
            }

            return to;
        }
    }
}