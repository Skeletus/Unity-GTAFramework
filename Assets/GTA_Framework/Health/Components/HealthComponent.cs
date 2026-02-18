using System;
using UnityEngine;

namespace GTAFramework.Health.Components
{
    /// <summary>
    /// Componente de salud minimalista que maneja vida, armadura y daño.
    /// Keep it simple, escalar cuando sea necesario.
    /// </summary>
    [DisallowMultipleComponent]
    public class HealthComponent : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField, Min(1f)] private float _maxHealth = 100f;
        [SerializeField] private bool _startWithMaxHealth = true;

        [Header("Armor Settings")]
        [SerializeField, Min(0f)] private float _maxArmor = 100f;
        [SerializeField, Range(0f, 1f)] private float _armorAbsorption = 0.75f;
        [SerializeField, Min(0f)] private float _initialArmor = 0f;

        [Header("Regeneration")]
        [SerializeField] private bool _canRegenerate;
        [SerializeField, Min(0f)] private float _regenRate = 5f;
        [SerializeField, Min(0f)] private float _regenDelay = 3f;

        [Header("Invulnerability")]
        [SerializeField, Min(0f)] private float _damageInvulnerabilityTime = 0f;

        // Runtime State
        private float _currentHealth;
        private float _currentArmor;
        private float _regenTimer;
        private float _invulnerabilityTimer;
        private bool _isAlive;

        // Events
        public event Action<float> OnDamageTaken;
        public event Action<float> OnHealed;
        public event Action<float> OnArmorChanged;
        public event Action OnDeath;
        public event Action OnRevived;

        // Properties
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public float CurrentArmor => _currentArmor;
        public float MaxArmor => _maxArmor;
        public bool IsAlive => _isAlive;
        public bool HasArmor => _currentArmor > 0f;
        public bool IsInvulnerable => _invulnerabilityTimer > 0f;
        public float HealthPercent => _maxHealth > 0f ? _currentHealth / _maxHealth : 0f;
        public float ArmorPercent => _maxArmor > 0f ? _currentArmor / _maxArmor : 0f;

        private void Awake()
        {
            if (_startWithMaxHealth)
            {
                _currentHealth = _maxHealth;
            }
            _currentArmor = Mathf.Clamp(_initialArmor, 0f, _maxArmor);
            _isAlive = _currentHealth > 0f;
        }

        private void Update()
        {
            UpdateInvulnerability(Time.deltaTime);
            TryRegenerate(Time.deltaTime);
        }

        #region Public API

        /// <summary>
        /// Aplica daño al componente. La armadura absorbe parte del daño.
        /// </summary>
        public void TakeDamage(float amount)
        {
            if (!_isAlive || amount <= 0f || IsInvulnerable) return;

            // Aplicar absorción de armadura
            if (HasArmor)
            {
                float absorbed = amount * _armorAbsorption;
                float armorUsed = Mathf.Min(_currentArmor, absorbed);
                _currentArmor -= armorUsed;
                amount -= armorUsed;

                OnArmorChanged?.Invoke(_currentArmor);
            }

            // Aplicar daño restante a la salud
            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            _regenTimer = _regenDelay;

            // Invulnerabilidad temporal
            if (_damageInvulnerabilityTime > 0f)
            {
                _invulnerabilityTimer = _damageInvulnerabilityTime;
            }

            OnDamageTaken?.Invoke(amount);

            if (_currentHealth <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Aplica daño con información extendida (para futura escalabilidad).
        /// </summary>
        public void TakeDamage(float amount, DamageType type, GameObject source = null)
        {
            // Por ahora igual que TakeDamage simple
            // Escalar: añadir multiplicadores por tipo de daño
            TakeDamage(amount);
        }

        /// <summary>
        /// Restaura salud.
        /// </summary>
        public void Heal(float amount)
        {
            if (!_isAlive || amount <= 0f) return;

            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            float healed = _currentHealth - previousHealth;

            if (healed > 0f)
            {
                OnHealed?.Invoke(healed);
            }
        }

        /// <summary>
        /// Añade armadura.
        /// </summary>
        public void AddArmor(float amount)
        {
            if (amount <= 0f) return;

            _currentArmor = Mathf.Min(_maxArmor, _currentArmor + amount);
            OnArmorChanged?.Invoke(_currentArmor);
        }

        /// <summary>
        /// Mata al personaje.
        /// </summary>
        public void Die()
        {
            if (!_isAlive) return;

            _isAlive = false;
            _currentHealth = 0f;
            OnDeath?.Invoke();
        }

        /// <summary>
        /// Revive al personaje con la salud especificada.
        /// </summary>
        public void Revive(float healthPercent = 1f)
        {
            if (_isAlive) return;

            _isAlive = true;
            _currentHealth = _maxHealth * Mathf.Clamp01(healthPercent);
            _regenTimer = 0f;
            OnRevived?.Invoke();
        }

        /// <summary>
        /// Restablece la salud al máximo.
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            _currentArmor = _initialArmor;
            _isAlive = true;
            _regenTimer = 0f;
            _invulnerabilityTimer = 0f;
        }

        /// <summary>
        /// Establece invulnerabilidad temporal.
        /// </summary>
        public void SetInvulnerable(float duration)
        {
            _invulnerabilityTimer = Mathf.Max(_invulnerabilityTimer, duration);
        }

        #endregion

        #region Private Methods

        private void UpdateInvulnerability(float deltaTime)
        {
            if (_invulnerabilityTimer > 0f)
            {
                _invulnerabilityTimer -= deltaTime;
            }
        }

        private void TryRegenerate(float deltaTime)
        {
            if (!_canRegenerate || !_isAlive || _regenRate <= 0f) return;
            if (_currentHealth >= _maxHealth) return;

            _regenTimer -= deltaTime;
            if (_regenTimer <= 0f)
            {
                Heal(_regenRate * deltaTime);
            }
        }

        #endregion
    }

    /// <summary>
    /// Tipos de daño básicos. Escalar según necesidades.
    /// </summary>
    public enum DamageType
    {
        General,
        Bullet,
        Melee,
        Explosion,
        Fall,
        Fire,
        Drowning,
        Vehicle
    }
}
