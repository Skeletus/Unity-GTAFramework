using GTAFramework.Health.Data;
using UnityEngine;

namespace GTAFramework.Health.Interfaces
{
    public interface IDamageable
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        float CurrentArmor { get; }      // Armadura actual
        float MaxArmor { get; }          // Armadura máxima
        bool IsAlive { get; }
        bool IsInvulnerable { get; set; }
        bool HasArmor { get; }           // ¿Tiene armadura equipada?

        void TakeDamage(DamageInfo damage);
        void Heal(float amount);
        void AddArmor(float amount);     // Añadir armadura
        void Kill();
    }
}