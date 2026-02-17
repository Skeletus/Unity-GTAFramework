using GTAFramework.Health.Data;
using UnityEngine;

namespace GTAFramework.Health.Interfaces
{
    public interface IArmor
    {
        float CurrentArmor { get; }
        float MaxArmor { get; }
        float DamageAbsorption { get; }  // Porcentaje de daño que absorbe (0-1)
        ArmorType ArmorType { get; }     // Light, Medium, Heavy

        void AddArmor(float amount);
        void TakeDamage(ref DamageInfo damage);  // Modifica el daño pasado
        bool HasArmor { get; }
    }
}