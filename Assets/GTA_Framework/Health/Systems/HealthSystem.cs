using System.Collections.Generic;
using GTAFramework.Core.Container;
using GTAFramework.Health.Interfaces;
using UnityEngine;

namespace GTAFramework.Health.Systems
{
    /// <summary>
    /// Central health system that tracks registered <see cref="IDamageable"/> entities
    /// and resolves damageable targets from colliders.
    /// </summary>
    [AutoRegister(Priority = 12, StartActive = true)]
    public class HealthSystem : IHealthSystem
    {
        private readonly HashSet<IDamageable> _damageables = new();
        private readonly Dictionary<Collider, IDamageable> _colliderCache = new();

        private float _cleanupTimer;
        private const float CleanupIntervalSeconds = 1f;

        /// <inheritdoc />
        public bool IsActive { get; set; } = true;

        /// <inheritdoc />
        public void Initialize()
        {
            DIContainer.Instance.RegisterSingleton<IHealthSystem>(this);
            DIContainer.Instance.RegisterSingleton(this);
            Debug.Log("[HealthSystem] Initialized and registered in DIContainer.");
        }

        /// <inheritdoc />
        public void Tick(float deltaTime)
        {
            _cleanupTimer += Mathf.Max(0f, deltaTime);
            if (_cleanupTimer < CleanupIntervalSeconds)
            {
                return;
            }

            _cleanupTimer = 0f;
            CleanupInvalidEntries();
        }

        /// <inheritdoc />
        public void LateTick(float deltaTime)
        {
            // Intentionally empty.
        }

        /// <inheritdoc />
        public void FixedTick(float fixedDeltaTime)
        {
            // Intentionally empty.
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            _damageables.Clear();
            _colliderCache.Clear();
            Debug.Log("[HealthSystem] Shutdown.");
        }

        /// <inheritdoc />
        public void RegisterDamageable(IDamageable damageable)
        {
            if (IsUnityObjectNull(damageable))
            {
                return;
            }

            _damageables.Add(damageable);
        }

        /// <inheritdoc />
        public void UnregisterDamageable(IDamageable damageable)
        {
            if (damageable == null)
            {
                return;
            }

            _damageables.Remove(damageable);
            RemoveFromColliderCache(damageable);
        }

        /// <inheritdoc />
        public IDamageable GetDamageableFromCollider(Collider collider)
        {
            if (collider == null)
            {
                return null;
            }

            if (_colliderCache.TryGetValue(collider, out IDamageable cached) && !IsUnityObjectNull(cached))
            {
                return cached;
            }

            IDamageable damageable = ResolveDamageableFromCollider(collider);
            if (damageable == null)
            {
                _colliderCache.Remove(collider);
                return null;
            }

            RegisterDamageable(damageable);
            _colliderCache[collider] = damageable;
            return damageable;
        }

        private IDamageable ResolveDamageableFromCollider(Collider collider)
        {
            IDamageable local = collider.GetComponent(typeof(IDamageable)) as IDamageable;
            if (!IsUnityObjectNull(local))
            {
                return local;
            }

            IDamageable parent = collider.GetComponentInParent(typeof(IDamageable)) as IDamageable;
            if (!IsUnityObjectNull(parent))
            {
                return parent;
            }

            return null;
        }

        private void CleanupInvalidEntries()
        {
            if (_damageables.Count > 0)
            {
                var staleDamageables = new List<IDamageable>();
                foreach (IDamageable damageable in _damageables)
                {
                    if (IsUnityObjectNull(damageable))
                    {
                        staleDamageables.Add(damageable);
                    }
                }

                for (int i = 0; i < staleDamageables.Count; i++)
                {
                    _damageables.Remove(staleDamageables[i]);
                }
            }

            if (_colliderCache.Count > 0)
            {
                var staleColliders = new List<Collider>();
                foreach (KeyValuePair<Collider, IDamageable> kvp in _colliderCache)
                {
                    if (kvp.Key == null || IsUnityObjectNull(kvp.Value))
                    {
                        staleColliders.Add(kvp.Key);
                    }
                }

                for (int i = 0; i < staleColliders.Count; i++)
                {
                    _colliderCache.Remove(staleColliders[i]);
                }
            }
        }

        private void RemoveFromColliderCache(IDamageable damageable)
        {
            if (_colliderCache.Count == 0)
            {
                return;
            }

            var keysToRemove = new List<Collider>();
            foreach (KeyValuePair<Collider, IDamageable> kvp in _colliderCache)
            {
                if (ReferenceEquals(kvp.Value, damageable) || kvp.Value == damageable)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            for (int i = 0; i < keysToRemove.Count; i++)
            {
                _colliderCache.Remove(keysToRemove[i]);
            }
        }

        private static bool IsUnityObjectNull(object instance)
        {
            if (instance == null)
            {
                return true;
            }

            if (instance is Object unityObject)
            {
                return unityObject == null;
            }

            return false;
        }
    }
}
