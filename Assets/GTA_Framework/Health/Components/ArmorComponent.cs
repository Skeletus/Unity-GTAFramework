using System;
using GTAFramework.Health.Data;
using GTAFramework.Health.Interfaces;
using GTAFramework.Health.Events;
using UnityEngine;

namespace GTAFramework.Health.Components
{
    /// <summary>
    /// Armor component used by <see cref="HealthComponent"/> to absorb incoming damage.
    /// </summary>
    [DisallowMultipleComponent]
    public class ArmorComponent : MonoBehaviour, IArmor
    {
        [Header("Configuration")]
        [SerializeField] private ArmorData _armorData;

        [Header("Runtime")]
        [SerializeField, Min(0f)] private float _initialArmor = 100f;

        private float _currentArmor;

        /// <summary>
        /// Raised when armor amount changes.
        /// The value is the current armor.
        /// </summary>
        public event Action<float> OnArmorChanged;

        /// <inheritdoc />
        public float CurrentArmor => _currentArmor;

        /// <inheritdoc />
        public float MaxArmor => _armorData != null ? Mathf.Max(0f, _armorData.maxArmor) : 0f;

        /// <inheritdoc />
        public float DamageAbsorption => _armorData != null ? Mathf.Clamp01(_armorData.damageAbsorption) : 0f;

        /// <inheritdoc />
        public ArmorType ArmorType => _armorData != null ? _armorData.armorType : ArmorType.None;

        /// <inheritdoc />
        public bool HasArmor => _currentArmor > 0f;

        private void Awake()
        {
            _currentArmor = Mathf.Clamp(_initialArmor, 0f, MaxArmor);
        }

        /// <inheritdoc />
        public void AddArmor(float amount)
        {
            if (amount <= 0f || MaxArmor <= 0f)
            {
                return;
            }

            float previousArmor = _currentArmor;
            _currentArmor = Mathf.Clamp(_currentArmor + amount, 0f, MaxArmor);

            if (!Mathf.Approximately(previousArmor, _currentArmor))
            {
                NotifyArmorChanged();
            }
        }

        /// <inheritdoc />
        public void TakeDamage(ref DamageInfo damage)
        {
            if (!HasArmor || damage.Amount <= 0f)
            {
                return;
            }

            float penetration = Mathf.Clamp01(damage.ArmorPenetration);
            float effectiveAbsorption = DamageAbsorption * (1f - penetration);
            if (effectiveAbsorption <= 0f)
            {
                return;
            }

            float requestedAbsorbedDamage = damage.Amount * effectiveAbsorption;
            if (requestedAbsorbedDamage <= 0f)
            {
                return;
            }

            float durabilityLoss = _armorData != null ? Mathf.Max(0f, _armorData.durabilityLossPerDamage) : 1f;
            float armorCost = requestedAbsorbedDamage * durabilityLoss;
            if (armorCost <= 0f)
            {
                return;
            }

            float previousArmor = _currentArmor;
            float armorSpent = Mathf.Min(_currentArmor, armorCost);
            float absorbRatio = armorSpent / armorCost;
            float realAbsorbedDamage = requestedAbsorbedDamage * absorbRatio;

            _currentArmor -= armorSpent;
            damage.Amount = Mathf.Max(0f, damage.Amount - realAbsorbedDamage);

            if (!Mathf.Approximately(previousArmor, _currentArmor))
            {
                NotifyArmorChanged();
            }
        }

        private void NotifyArmorChanged()
        {
            OnArmorChanged?.Invoke(_currentArmor);
            HealthEvents.RaiseArmorChanged(gameObject, _currentArmor, MaxArmor);
        }
    }
}
