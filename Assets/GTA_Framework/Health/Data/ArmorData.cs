using UnityEngine;

namespace GTAFramework.Health.Data
{
    [CreateAssetMenu(fileName = "ArmorData", menuName = "GTA Framework/Health/ArmorData")]
    public class ArmorData : ScriptableObject
    {
        public ArmorType armorType = ArmorType.Medium;
        public float maxArmor = 100f;
        public float damageAbsorption = 0.75f;  // 75% del daño va a la armadura
        public float durabilityLossPerDamage = 0.5f;  // La armadura se desgasta
    }
}