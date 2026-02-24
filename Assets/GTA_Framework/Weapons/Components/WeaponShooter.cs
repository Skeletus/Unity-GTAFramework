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
        [Tooltip("Si esta asignado, el origen del raycast sera este.")]
        [SerializeField] private Transform _fireOriginOverride;

        [Header("Aim Target")]
        [Tooltip("Referencia de aim en el mundo (esfera). Si no se pasa un aimTarget, se usa este.")]
        [SerializeField] private Transform _aimTarget;

        [Header("Debug")]
        [SerializeField] private bool _drawDebugRay = false;
        [SerializeField, Min(0f)] private float _debugRayDuration = 0.05f;

        private float _nextShotTime;

        /// <summary>
        /// Intenta disparar. Requiere un arma de fuego equipada.
        /// </summary>
        public bool TryShoot(WeaponData weapon, Transform fireOrigin, Transform aimTarget, GameObject owner)
        {
            if (weapon == null || !weapon.isFirearm)
                return false;

            if (weapon.fireRate <= 0f)
                return false;

            if (Time.time < _nextShotTime)
                return false;

            _nextShotTime = Time.time + (1f / weapon.fireRate);

            Transform origin = _fireOriginOverride != null ? _fireOriginOverride : fireOrigin;
            if (origin == null)
                return false;

            Transform finalAimTarget = aimTarget != null ? aimTarget : _aimTarget;
            Vector3 direction = GetAimDirection(origin, finalAimTarget, fireOrigin);
            Vector3 start = origin.position;

            if (_drawDebugRay)
                Debug.DrawRay(start, direction * weapon.range, Color.red, _debugRayDuration);

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

        private static Vector3 GetAimDirection(Transform origin, Transform aimTarget, Transform fallbackForward)
        {
            if (aimTarget != null)
            {
                Vector3 toTarget = aimTarget.position - origin.position;
                if (toTarget.sqrMagnitude > 0.0001f)
                    return toTarget.normalized;
            }

            if (fallbackForward != null)
                return fallbackForward.forward.normalized;

            return origin.forward.normalized;
        }
    }
}
