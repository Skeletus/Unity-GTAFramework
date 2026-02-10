using UnityEngine;

namespace GTAFramework.GTA_Animation.Data
{
    [System.Serializable]
    public class LocomotionTuning
    {
        [Header("Animator Damping")]
        [Min(0f)] public float speedDampTime = 0.08f;

        [Header("Speed Mapping")]
        [Tooltip("Escala opcional por si tu velocidad del gameplay no coincide con tu BlendTree.")]
        [Min(0.01f)] public float speedMultiplier = 1f;

        [Tooltip("Velocidad mínima para considerar que hay movimiento.")]
        [Min(0f)] public float moveEpsilon = 0.05f;
    }
}
