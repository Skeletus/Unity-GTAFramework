using UnityEngine;
using GTAFramework.Health.Components;

namespace GTAFramework.Health.Interfaces
{
    /// <summary>
    /// Contrato para objetos que pueden recibir dano.
    /// </summary>
    public interface IDamageable
    {
        void ApplyDamage(float amount, DamageType type, GameObject source = null);
    }
}
