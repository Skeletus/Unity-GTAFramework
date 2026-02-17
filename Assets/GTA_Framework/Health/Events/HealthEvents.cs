using System;
using GTAFramework.Health.Data;
using UnityEngine;

namespace GTAFramework.Health.Events
{
    /// <summary>
    /// Payload for damage notifications.
    /// </summary>
    public readonly struct DamageEventData
    {
        /// <summary>
        /// Creates a new damage event payload.
        /// </summary>
        public DamageEventData(GameObject target, DamageInfo damageInfo)
        {
            Target = target;
            DamageInfo = damageInfo;
        }

        /// <summary>
        /// Target GameObject that received damage.
        /// </summary>
        public GameObject Target { get; }

        /// <summary>
        /// Damage payload after processing.
        /// </summary>
        public DamageInfo DamageInfo { get; }
    }

    /// <summary>
    /// Payload for heal notifications.
    /// </summary>
    public readonly struct HealEventData
    {
        /// <summary>
        /// Creates a new heal event payload.
        /// </summary>
        public HealEventData(GameObject target, float amount)
        {
            Target = target;
            Amount = amount;
        }

        /// <summary>
        /// Target GameObject that was healed.
        /// </summary>
        public GameObject Target { get; }

        /// <summary>
        /// Effective heal amount.
        /// </summary>
        public float Amount { get; }
    }

    /// <summary>
    /// Payload for death notifications.
    /// </summary>
    public readonly struct DeathEventData
    {
        /// <summary>
        /// Creates a new death event payload.
        /// </summary>
        public DeathEventData(GameObject target)
        {
            Target = target;
        }

        /// <summary>
        /// Target GameObject that died.
        /// </summary>
        public GameObject Target { get; }
    }

    /// <summary>
    /// Payload for armor change notifications.
    /// </summary>
    public readonly struct ArmorChangedEventData
    {
        /// <summary>
        /// Creates a new armor changed payload.
        /// </summary>
        public ArmorChangedEventData(GameObject target, float currentArmor, float maxArmor)
        {
            Target = target;
            CurrentArmor = currentArmor;
            MaxArmor = maxArmor;
        }

        /// <summary>
        /// Target GameObject whose armor changed.
        /// </summary>
        public GameObject Target { get; }

        /// <summary>
        /// New current armor value.
        /// </summary>
        public float CurrentArmor { get; }

        /// <summary>
        /// Maximum armor value.
        /// </summary>
        public float MaxArmor { get; }
    }

    /// <summary>
    /// Global health event hub for systems such as UI, audio, and VFX.
    /// </summary>
    public static class HealthEvents
    {
        /// <summary>
        /// Raised when a target takes damage.
        /// </summary>
        public static event Action<DamageEventData> OnDamage;

        /// <summary>
        /// Raised when a target is healed.
        /// </summary>
        public static event Action<HealEventData> OnHeal;

        /// <summary>
        /// Raised when a target dies.
        /// </summary>
        public static event Action<DeathEventData> OnDeath;

        /// <summary>
        /// Raised when target armor changes.
        /// </summary>
        public static event Action<ArmorChangedEventData> OnArmorChanged;

        /// <summary>
        /// Raises a damage event.
        /// </summary>
        public static void RaiseDamage(GameObject target, DamageInfo damageInfo)
        {
            OnDamage?.Invoke(new DamageEventData(target, damageInfo));
        }

        /// <summary>
        /// Raises a heal event.
        /// </summary>
        public static void RaiseHeal(GameObject target, float amount)
        {
            OnHeal?.Invoke(new HealEventData(target, amount));
        }

        /// <summary>
        /// Raises a death event.
        /// </summary>
        public static void RaiseDeath(GameObject target)
        {
            OnDeath?.Invoke(new DeathEventData(target));
        }

        /// <summary>
        /// Raises an armor changed event.
        /// </summary>
        public static void RaiseArmorChanged(GameObject target, float currentArmor, float maxArmor)
        {
            OnArmorChanged?.Invoke(new ArmorChangedEventData(target, currentArmor, maxArmor));
        }

        /// <summary>
        /// Clears all event subscriptions.
        /// Useful on domain reload boundaries in editor tooling/tests.
        /// </summary>
        public static void Reset()
        {
            OnDamage = null;
            OnHeal = null;
            OnDeath = null;
            OnArmorChanged = null;
        }
    }
}
