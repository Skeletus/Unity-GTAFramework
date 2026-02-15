using UnityEngine;

namespace GTAFramework.GTACamera.Interfaces
{
    /// <summary>
    /// Estrategia para manejar colisiones de la cámara con el entorno.
    /// Permite diferentes implementaciones (SphereCast, Raycast, etc.)
    /// </summary>
    public interface ICameraCollisionHandler
    {
        /// <summary>
        /// Calcula la posición final de la cámara evitando colisiones.
        /// </summary>
        /// <param name="from">Posición del pivot (origen del cast)</param>
        /// <param name="to">Posición deseada de la cámara</param>
        /// <param name="radius">Radio de la esfera para el SphereCast</param>
        /// <param name="layers">Capas a considerar para colisión</param>
        /// <param name="offset">Offset para evitar clipping</param>
        /// <param name="minDistance">Distancia mínima permitida</param>
        /// <returns>Posición ajustada de la cámara</returns>
        Vector3 HandleCollision(
            Vector3 from,
            Vector3 to,
            float radius,
            LayerMask layers,
            float offset,
            float minDistance
        );
    }
}