using UnityEngine;
using GTAFramework.Health.Components;
using GTAFramework.Health.Interfaces;
using GTAFramework.Weapons.Data;

namespace GTAFramework.Weapons.Components
{
    /// <summary>
    /// Logica de disparo hitscan con cadencia y dano configurables por arma.
    /// </summary>
    [DisallowMultipleComponent]
    public class WeaponShooter : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private LayerMask _hitMask = ~0;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("Fire Origin")]
        [Tooltip("Si esta asignado, el origen del raycast sera este. La direccion seguira la camara si se provee.")]
        [SerializeField] private Transform _fireOriginOverride;

        [Header("Debug")]
        [SerializeField] private bool _drawDebugRay = false;
        [SerializeField, Min(0f)] private float _debugRayDuration = 0.05f;

        private float _nextShotTime;

        /// <summary>
        /// Intenta disparar. Requiere un arma de fuego equipada.
        /// </summary>
        public bool TryShoot(WeaponData weapon, Transform fallbackAimOrigin, GameObject owner)
        {
            if (weapon == null || !weapon.isFirearm)
                return false;

            if (weapon.fireRate <= 0f)
                return false;

            if (Time.time < _nextShotTime)
                return false;

            _nextShotTime = Time.time + (1f / weapon.fireRate);

            Transform aimOrigin = fallbackAimOrigin != null ? fallbackAimOrigin : _fireOriginOverride;
            Transform origin = _fireOriginOverride != null ? _fireOriginOverride : fallbackAimOrigin;

            if (origin == null)
                return false;

            Vector3 direction = aimOrigin != null ? aimOrigin.forward : origin.forward;
            Vector3 start = origin.position;

            if (_drawDebugRay)
                Debug.DrawRay(start, direction.normalized * weapon.range, Color.red, _debugRayDuration);

            if (Physics.Raycast(start, direction, out RaycastHit hit, weapon.range, _hitMask, _triggerInteraction))
            {
                var damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.ApplyDamage(weapon.damage, DamageType.Bullet, owner);
                }
            }

            return true;
        }
    }
}
