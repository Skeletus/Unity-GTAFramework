using GTAFramework.Health.Data;
using UnityEngine;

namespace GTAFramework.Health.Interfaces
{
    public interface IDamageSource
    {
        float BaseDamage { get; }
        DamageType DamageType { get; }
        GameObject SourceObject { get; }

        DamageInfo CreateDamageInfo(IDamageable target);
    }
}