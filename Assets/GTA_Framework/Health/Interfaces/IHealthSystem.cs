using GTAFramework.Core.Interfaces;
using UnityEngine;

namespace GTAFramework.Health.Interfaces
{
    public interface IHealthSystem : IGameSystem
    {
        void RegisterDamageable(IDamageable damageable);
        void UnregisterDamageable(IDamageable damageable);
        IDamageable GetDamageableFromCollider(Collider collider);
    }
}
