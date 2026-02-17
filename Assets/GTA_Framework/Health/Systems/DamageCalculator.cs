using GTAFramework.Health.Data;
using GTAFramework.Health.Interfaces;
using UnityEngine;

namespace GTAFramework.Health.Systems
{
    /// <summary>
    /// Utility class for building and normalizing damage payloads.
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>
        /// Creates a <see cref="DamageInfo"/> from primitive input.
        /// </summary>
        public static DamageInfo CreateDamage(
            float amount,
            DamageType type,
            GameObject source,
            GameObject target,
            Vector3 hitPoint,
            Vector3 hitDirection,
            float force = 0f,
            float armorPenetration = 0f)
        {
            DamageInfo damage = new DamageInfo
            {
                Amount = amount,
                Type = type,
                Source = source,
                Target = target,
                HitPoint = hitPoint,
                HitDirection = hitDirection,
                Force = force,
                ArmorPenetration = armorPenetration
            };

            return Sanitize(damage);
        }

        /// <summary>
        /// Builds damage from a source object against a target.
        /// </summary>
        public static DamageInfo CreateFromSource(IDamageSource source, IDamageable target, GameObject fallbackTarget = null)
        {
            if (source == null)
            {
                return default;
            }

            DamageInfo damage = source.CreateDamageInfo(target);
            if (damage.Source == null)
            {
                damage.Source = source.SourceObject;
            }

            if (damage.Target == null)
            {
                damage.Target = fallbackTarget;
            }

            return Sanitize(damage);
        }

        /// <summary>
        /// Applies an animation curve multiplier based on <see cref="DamageType"/> index.
        /// </summary>
        public static DamageInfo ApplyTypeCurve(DamageInfo damage, AnimationCurve curve)
        {
            if (curve == null || curve.length == 0)
            {
                return Sanitize(damage);
            }

            int maxIndex = System.Enum.GetValues(typeof(DamageType)).Length - 1;
            float normalizedType = maxIndex > 0 ? Mathf.Clamp01((float)damage.Type / maxIndex) : 0f;
            float multiplier = curve.Evaluate(normalizedType);

            damage.Amount = Mathf.Max(0f, damage.Amount * Mathf.Max(0f, multiplier));
            return Sanitize(damage);
        }

        /// <summary>
        /// Converts collision impulse into gameplay damage.
        /// </summary>
        public static float CalculateCollisionDamage(
            float impulseMagnitude,
            float minImpulse,
            float multiplier,
            float maxDamage)
        {
            float clampedMinImpulse = Mathf.Max(0f, minImpulse);
            float clampedMultiplier = Mathf.Max(0f, multiplier);
            float clampedMaxDamage = Mathf.Max(0f, maxDamage);

            if (impulseMagnitude <= clampedMinImpulse || clampedMultiplier <= 0f || clampedMaxDamage <= 0f)
            {
                return 0f;
            }

            float rawDamage = (impulseMagnitude - clampedMinImpulse) * clampedMultiplier;
            return Mathf.Clamp(rawDamage, 0f, clampedMaxDamage);
        }

        /// <summary>
        /// Clamps damage fields to safe ranges.
        /// </summary>
        public static DamageInfo Sanitize(DamageInfo damage)
        {
            damage.Amount = Mathf.Max(0f, damage.Amount);
            damage.Force = Mathf.Max(0f, damage.Force);
            damage.ArmorPenetration = Mathf.Clamp01(damage.ArmorPenetration);

            if (damage.HitDirection.sqrMagnitude > 0.000001f)
            {
                damage.HitDirection = damage.HitDirection.normalized;
            }

            return damage;
        }
    }
}
