using UnityEngine;

namespace GTAFramework.Health.Data
{
    public struct DamageInfo
    {
        public float Amount;
        public DamageType Type;
        public Vector3 HitPoint;
        public Vector3 HitDirection;
        public GameObject Source;
        public GameObject Target;
        public float Force;           // Para knockback
        public float ArmorPenetration; // Porcentaje que ignora armadura (0-1)
    }
}