using GTAFramework.Health.Data;
using GTAFramework.Health.Interfaces;
using GTAFramework.Health.Systems;
using UnityEngine;

namespace GTAFramework.Health.Components
{
    /// <summary>
    /// Receives damage requests and forwards them to an <see cref="IDamageable"/> target.
    /// Can consume explicit damage sources and optional collision-based damage.
    /// </summary>
    [DisallowMultipleComponent]
    public class DamageReceiver : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private MonoBehaviour _damageableOverride;
        [SerializeField] private bool _searchInParents = true;

        [Header("Auto Source Detection")]
        [SerializeField] private bool _detectDamageSourceOnCollision = true;
        [SerializeField] private bool _detectDamageSourceOnTrigger = true;

        [Header("Collision Damage")]
        [SerializeField] private bool _enableCollisionDamage = true;
        [SerializeField] private DamageType _collisionDamageType = DamageType.Vehicle;
        [SerializeField, Min(0f)] private float _collisionMinImpulse = 4f;
        [SerializeField, Min(0f)] private float _collisionDamageMultiplier = 0.15f;
        [SerializeField, Min(0f)] private float _maxCollisionDamage = 80f;
        [SerializeField, Range(0f, 1f)] private float _collisionArmorPenetration = 0f;

        private IDamageable _damageable;

        private void Awake()
        {
            ResolveDamageable();
        }

        /// <summary>
        /// Applies damage directly to the configured damageable target.
        /// </summary>
        /// <param name="damage">Damage payload.</param>
        public void ReceiveDamage(DamageInfo damage)
        {
            if (!TryEnsureDamageable())
            {
                return;
            }

            DamageInfo finalDamage = DamageCalculator.Sanitize(damage);
            finalDamage.Target = GetDamageableGameObject();
            _damageable.TakeDamage(finalDamage);
        }

        /// <summary>
        /// Convenience overload to create and apply damage in one call.
        /// </summary>
        /// <param name="amount">Raw damage amount.</param>
        /// <param name="type">Damage type.</param>
        /// <param name="source">Attacker/source object.</param>
        /// <param name="hitPoint">Optional impact world position.</param>
        /// <param name="hitDirection">Optional direction from source to target.</param>
        /// <param name="force">Optional impulse force value.</param>
        /// <param name="armorPenetration">Armor penetration in [0,1].</param>
        public void ReceiveDamage(
            float amount,
            DamageType type,
            GameObject source = null,
            Vector3 hitPoint = default,
            Vector3 hitDirection = default,
            float force = 0f,
            float armorPenetration = 0f)
        {
            DamageInfo damage = DamageCalculator.CreateDamage(
                amount,
                type,
                source,
                GetDamageableGameObject(),
                hitPoint,
                hitDirection,
                force,
                armorPenetration);

            ReceiveDamage(damage);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_detectDamageSourceOnCollision)
            {
                IDamageSource source = FindDamageSource(collision.collider);
                if (source != null)
                {
                    Vector3 contactPoint = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
                    ApplyDamageFromSource(source, contactPoint, collision.relativeVelocity.normalized);
                    return;
                }
            }

            if (_enableCollisionDamage)
            {
                float impulse = collision.impulse.magnitude;
                float amount = DamageCalculator.CalculateCollisionDamage(
                    impulse,
                    _collisionMinImpulse,
                    _collisionDamageMultiplier,
                    _maxCollisionDamage);

                if (amount <= 0f)
                {
                    return;
                }

                Vector3 contactPoint = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
                DamageInfo collisionDamage = DamageCalculator.CreateDamage(
                    amount,
                    _collisionDamageType,
                    collision.gameObject,
                    GetDamageableGameObject(),
                    contactPoint,
                    collision.relativeVelocity.normalized,
                    impulse,
                    _collisionArmorPenetration);

                ReceiveDamage(collisionDamage);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_detectDamageSourceOnTrigger)
            {
                return;
            }

            IDamageSource source = FindDamageSource(other);
            if (source == null)
            {
                return;
            }

            Vector3 direction = other.transform.position != transform.position
                ? (transform.position - other.transform.position).normalized
                : Vector3.zero;

            ApplyDamageFromSource(source, transform.position, direction);
        }

        private void ApplyDamageFromSource(IDamageSource source, Vector3 hitPoint, Vector3 hitDirection)
        {
            if (!TryEnsureDamageable() || source == null)
            {
                return;
            }

            DamageInfo damage = DamageCalculator.CreateFromSource(source, _damageable, GetDamageableGameObject());
            damage.HitPoint = hitPoint;
            damage.HitDirection = hitDirection;

            ReceiveDamage(damage);
        }

        private IDamageSource FindDamageSource(Component fromComponent)
        {
            if (fromComponent == null)
            {
                return null;
            }

            IDamageSource source = fromComponent.GetComponent(typeof(IDamageSource)) as IDamageSource;
            if (source != null)
            {
                return source;
            }

            if (_searchInParents)
            {
                return fromComponent.GetComponentInParent(typeof(IDamageSource)) as IDamageSource;
            }

            return null;
        }

        private void ResolveDamageable()
        {
            _damageable = _damageableOverride as IDamageable;
            if (_damageable != null)
            {
                return;
            }

            _damageable = GetComponent(typeof(IDamageable)) as IDamageable;
            if (_damageable != null)
            {
                return;
            }

            if (_searchInParents)
            {
                _damageable = GetComponentInParent(typeof(IDamageable)) as IDamageable;
            }
        }

        private bool TryEnsureDamageable()
        {
            if (_damageable != null)
            {
                return true;
            }

            ResolveDamageable();
            if (_damageable != null)
            {
                return true;
            }

            Debug.LogWarning($"[{nameof(DamageReceiver)}] No IDamageable found for {name}.", this);
            return false;
        }

        private GameObject GetDamageableGameObject()
        {
            if (_damageable is Component component && component != null)
            {
                return component.gameObject;
            }

            return gameObject;
        }
    }
}
