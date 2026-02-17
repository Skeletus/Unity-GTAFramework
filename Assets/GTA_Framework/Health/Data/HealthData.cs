using UnityEngine;

namespace GTAFramework.Health.Data
{
    [CreateAssetMenu(fileName = "HealthData", menuName = "GTA Framework/Health/HealthData")]
    public class HealthData : ScriptableObject
    {
        public float maxHealth = 100f;
        public float healthRegenRate = 0f;
        public float regenDelay = 5f;
        public bool canRegen = false;
        public AnimationCurve damageCurve;  // Modificadores por tipo de daño
    }
}