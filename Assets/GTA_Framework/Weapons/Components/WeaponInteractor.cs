using System.Collections.Generic;
using UnityEngine;

namespace GTAFramework.Weapons.Components
{
    /// <summary>
    /// Mantiene las armas cercanas y permite interactuar con la mejor candidata.
    /// </summary>
    [DisallowMultipleComponent]
    public class WeaponInteractor : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField, Min(0.1f)] private float _maxPickupDistance = 2.5f;

        private readonly List<WeaponPickup> _nearbyPickups = new();

        public void RegisterPickup(WeaponPickup pickup)
        {
            if (pickup == null)
                return;

            if (!_nearbyPickups.Contains(pickup))
                _nearbyPickups.Add(pickup);
        }

        public void UnregisterPickup(WeaponPickup pickup)
        {
            if (pickup == null)
                return;

            _nearbyPickups.Remove(pickup);
        }

        /// <summary>
        /// Intenta recoger la mejor arma disponible (más cercana).
        /// </summary>
        public bool TryPickup(WeaponInventory inventory)
        {
            if (inventory == null)
                return false;

            WeaponPickup best = GetBestPickup();
            if (best == null)
                return false;

            return best.TryPickup(inventory);
        }

        private WeaponPickup GetBestPickup()
        {
            if (_nearbyPickups.Count == 0)
                return null;

            WeaponPickup best = null;
            float bestDistSq = float.PositiveInfinity;
            Vector3 pos = transform.position;
            float maxDistSq = _maxPickupDistance * _maxPickupDistance;

            // Limpieza mínima de referencias muertas
            for (int i = _nearbyPickups.Count - 1; i >= 0; i--)
            {
                var p = _nearbyPickups[i];
                if (p == null)
                {
                    _nearbyPickups.RemoveAt(i);
                    continue;
                }

                if (!p.IsAvailable)
                    continue;

                float distSq = (p.transform.position - pos).sqrMagnitude;
                if (distSq > maxDistSq)
                    continue;

                if (distSq < bestDistSq)
                {
                    best = p;
                    bestDistSq = distSq;
                }
            }

            return best;
        }
    }
}
