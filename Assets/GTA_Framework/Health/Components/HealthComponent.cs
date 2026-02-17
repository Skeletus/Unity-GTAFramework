using System;
using GTAFramework.Core.Container;
using GTAFramework.Health.Data;
using GTAFramework.Health.Interfaces;
using GTAFramework.Health.Events;
using UnityEngine;

namespace GTAFramework.Health.Components
{
    /// <summary>
    /// Main health component for the framework.
    /// Handles damage, healing, armor, regeneration, and temporary invulnerability.
    /// </summary>
    [DisallowMultipleComponent]
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [Header("Configuration")]
        [SerializeField] private HealthData _healthData;

        [Header("Invulnerability")]
        [SerializeField, Min(0f)] private float _damageInvulnerabilityDuration = 0f;
        [SerializeField] private bool _startInvulnerable;

        [Inject] private IHealthSystem _healthSystem;

        private IArmor _armor;
        private float _currentHealth;
        private bool _isAlive;
        private bool _manualInvulnerability;
        private float _temporaryInvulnerabilityTimer;
        private float _nextRegenAllowedTime;
        private bool _isRegisteredInHealthSystem;

        /// <summary>
        /// Raised when this component takes damage.
        /// The <see cref="DamageInfo"/> payload contains the final health damage.
        /// </summary>
        public event Action<DamageInfo> OnDamage;

        /// <summary>
        /// Raised once when this component dies.
        /// </summary>
        public event Action<HealthComponent> OnDeath;

        /// <summary>
        /// Raised when health is restored.
        /// The value is the effective healed amount.
        /// </summary>
        public event Action<float> OnHeal;

        /// <summary>
        /// Raised when armor changes.
        /// The value is the current armor amount.
        /// </summary>
        public event Action<float> OnArmorChanged;

        /// <inheritdoc />
        public float CurrentHealth => _currentHealth;

        /// <inheritdoc />
        public float MaxHealth => _healthData != null ? Mathf.Max(0f, _healthData.maxHealth) : 0f;

        /// <inheritdoc />
        public float CurrentArmor => _armor != null ? _armor.CurrentArmor : 0f;

        /// <inheritdoc />
        public float MaxArmor => _armor != null ? _armor.MaxArmor : 0f;

        /// <inheritdoc />
        public bool IsAlive => _isAlive;

        /// <inheritdoc />
        public bool IsInvulnerable
        {
            get => _manualInvulnerability || _temporaryInvulnerabilityTimer > 0f;
            set
            {
                _manualInvulnerability = value;
                if (!value)
                {
                    _temporaryInvulnerabilityTimer = 0f;
                }
            }
        }

        /// <inheritdoc />
        public bool HasArmor => _armor != null && _armor.HasArmor;

        private void Awake()
        {
            DIContainer.Instance.InjectDependencies(this);

            _armor = GetComponent(typeof(IArmor)) as IArmor;
            ResetHealthState();

            if (_startInvulnerable)
            {
                IsInvulnerable = true;
            }
        }

        private void OnEnable()
        {
            TryRegisterInHealthSystem();
        }

        private void Start()
        {
            // Fallback in case health system was not available during Awake/OnEnable.
            TryRegisterInHealthSystem();
        }

        private void Update()
        {
            UpdateTemporaryInvulnerability(Time.deltaTime);
            TryRegenerateHealth(Time.deltaTime);
        }

        private void OnDisable()
        {
            if (_isRegisteredInHealthSystem && _healthSystem != null)
            {
                _healthSystem.UnregisterDamageable(this);
                _isRegisteredInHealthSystem = false;
            }
        }

        /// <inheritdoc />
        public void TakeDamage(DamageInfo damage)
        {
            if (!_isAlive || IsInvulnerable || damage.Amount <= 0f)
            {
                return;
            }

            DamageInfo finalDamage = ApplyDamageCurve(damage);
            finalDamage.Target = gameObject;

            float previousArmor = CurrentArmor;
            if (_armor != null && _armor.HasArmor)
            {
                _armor.TakeDamage(ref finalDamage);
                if (!Mathf.Approximately(previousArmor, CurrentArmor))
                {
                    OnArmorChanged?.Invoke(CurrentArmor);
                    HealthEvents.RaiseArmorChanged(gameObject, CurrentArmor, MaxArmor);
                }
            }

            finalDamage.Amount = Mathf.Max(0f, finalDamage.Amount);
            if (finalDamage.Amount <= 0f)
            {
                return;
            }

            _currentHealth = Mathf.Clamp(_currentHealth - finalDamage.Amount, 0f, MaxHealth);
            _nextRegenAllowedTime = Time.time + GetRegenDelay();

            if (_damageInvulnerabilityDuration > 0f)
            {
                SetTemporaryInvulnerability(_damageInvulnerabilityDuration);
            }

            OnDamage?.Invoke(finalDamage);
            HealthEvents.RaiseDamage(gameObject, finalDamage);

            if (_currentHealth <= 0f)
            {
                Kill();
            }
        }

        /// <inheritdoc />
        public void Heal(float amount)
        {
            if (!_isAlive || amount <= 0f)
            {
                return;
            }

            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Clamp(_currentHealth + amount, 0f, MaxHealth);

            float healedAmount = _currentHealth - previousHealth;
            if (healedAmount > 0f)
            {
                OnHeal?.Invoke(healedAmount);
                HealthEvents.RaiseHeal(gameObject, healedAmount);
            }
        }

        /// <inheritdoc />
        public void AddArmor(float amount)
        {
            if (_armor == null || amount <= 0f)
            {
                return;
            }

            float previousArmor = CurrentArmor;
            _armor.AddArmor(amount);

            if (!Mathf.Approximately(previousArmor, CurrentArmor))
            {
                OnArmorChanged?.Invoke(CurrentArmor);
                HealthEvents.RaiseArmorChanged(gameObject, CurrentArmor, MaxArmor);
            }
        }

        /// <inheritdoc />
        public void Kill()
        {
            if (!_isAlive)
            {
                return;
            }

            _currentHealth = 0f;
            _isAlive = false;
            OnDeath?.Invoke(this);
            HealthEvents.RaiseDeath(gameObject);
        }

        /// <summary>
        /// Enables temporary invulnerability for the given duration.
        /// </summary>
        /// <param name="duration">Duration in seconds.</param>
        public void SetTemporaryInvulnerability(float duration)
        {
            if (duration <= 0f)
            {
                return;
            }

            _temporaryInvulnerabilityTimer = Mathf.Max(_temporaryInvulnerabilityTimer, duration);
        }

        private void ResetHealthState()
        {
            _currentHealth = MaxHealth;
            _isAlive = _currentHealth > 0f;
            _temporaryInvulnerabilityTimer = 0f;
            _nextRegenAllowedTime = Time.time + GetRegenDelay();
        }

        private void TryRegisterInHealthSystem()
        {
            if (_isRegisteredInHealthSystem)
            {
                return;
            }

            if (_healthSystem == null)
            {
                _healthSystem = DIContainer.Instance.Resolve<IHealthSystem>();
            }

            if (_healthSystem == null)
            {
                return;
            }

            _healthSystem.RegisterDamageable(this);
            _isRegisteredInHealthSystem = true;
        }

        private void UpdateTemporaryInvulnerability(float deltaTime)
        {
            if (_temporaryInvulnerabilityTimer <= 0f)
            {
                return;
            }

            _temporaryInvulnerabilityTimer -= deltaTime;
            if (_temporaryInvulnerabilityTimer < 0f)
            {
                _temporaryInvulnerabilityTimer = 0f;
            }
        }

        private void TryRegenerateHealth(float deltaTime)
        {
            if (_healthData == null || !_healthData.canRegen || !_isAlive)
            {
                return;
            }

            if (_currentHealth >= MaxHealth || Time.time < _nextRegenAllowedTime)
            {
                return;
            }

            float regenRate = Mathf.Max(0f, _healthData.healthRegenRate);
            if (regenRate <= 0f)
            {
                return;
            }

            Heal(regenRate * deltaTime);
        }

        private DamageInfo ApplyDamageCurve(DamageInfo damage)
        {
            if (_healthData == null || _healthData.damageCurve == null || _healthData.damageCurve.length == 0)
            {
                return damage;
            }

            int maxIndex = Enum.GetValues(typeof(DamageType)).Length - 1;
            float normalizedType = maxIndex > 0 ? Mathf.Clamp01((float)damage.Type / maxIndex) : 0f;
            float multiplier = _healthData.damageCurve.Evaluate(normalizedType);

            DamageInfo modifiedDamage = damage;
            modifiedDamage.Amount = Mathf.Max(0f, damage.Amount * multiplier);
            return modifiedDamage;
        }

        private float GetRegenDelay()
        {
            return _healthData != null ? Mathf.Max(0f, _healthData.regenDelay) : 0f;
        }
    }
}
