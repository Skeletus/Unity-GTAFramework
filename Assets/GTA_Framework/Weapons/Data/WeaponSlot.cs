using System.Collections.Generic;
using UnityEngine;

namespace GTAFramework.Weapons.Data
{
    /// <summary>
    /// Representa un slot de armas que puede contener múltiples armas del mismo tipo,
    /// pero solo una puede estar activa a la vez.
    /// </summary>
    [System.Serializable]
    public class WeaponSlot
    {
        [SerializeField] private WeaponType _slotType;
        [SerializeField] private List<WeaponData> _ownedWeapons = new();
        [SerializeField] private WeaponData _activeWeapon;

        // Eventos
        public event System.Action<WeaponData> OnActiveWeaponChanged;
        public event System.Action<WeaponData> OnWeaponAdded;

        // Properties
        public WeaponType SlotType => _slotType;
        public WeaponData ActiveWeapon => _activeWeapon;
        public IReadOnlyList<WeaponData> OwnedWeapons => _ownedWeapons;
        public bool HasActiveWeapon => _activeWeapon != null;

        public WeaponSlot(WeaponType type)
        {
            _slotType = type;
        }

        /// <summary>
        /// ¿Puede agregar esta arma al slot?
        /// </summary>
        public bool CanAddWeapon(WeaponData weapon)
        {
            if (weapon == null) return false;
            if (weapon.weaponType != _slotType) return false;
            if (_ownedWeapons.Contains(weapon)) return false;
            if (_ownedWeapons.Count >= _slotType.maxWeaponsPerSlot) return false;
            return true;
        }

        /// <summary>
        /// Agrega un arma al slot
        /// </summary>
        public bool AddWeapon(WeaponData weapon)
        {
            if (!CanAddWeapon(weapon)) return false;

            _ownedWeapons.Add(weapon);

            // Si es la primera arma, la hacemos activa automáticamente
            if (_ownedWeapons.Count == 1)
            {
                SetActiveWeapon(weapon);
            }

            OnWeaponAdded?.Invoke(weapon);
            return true;
        }

        /// <summary>
        /// ¿Tiene esta arma en el slot?
        /// </summary>
        public bool HasWeapon(WeaponData weapon)
        {
            return _ownedWeapons.Contains(weapon);
        }

        /// <summary>
        /// Establece el arma activa de este slot
        /// </summary>
        public bool SetActiveWeapon(WeaponData weapon)
        {
            if (weapon == null)
            {
                _activeWeapon = null;
                OnActiveWeaponChanged?.Invoke(null);
                return true;
            }

            if (!_ownedWeapons.Contains(weapon)) return false;

            _activeWeapon = weapon;
            OnActiveWeaponChanged?.Invoke(weapon);
            return true;
        }

        /// <summary>
        /// Remueve un arma del slot
        /// </summary>
        public bool RemoveWeapon(WeaponData weapon)
        {
            if (!_ownedWeapons.Remove(weapon)) return false;

            // Si el arma removida era la activa, seleccionar otra
            if (_activeWeapon == weapon)
            {
                _activeWeapon = _ownedWeapons.Count > 0 ? _ownedWeapons[0] : null;
                OnActiveWeaponChanged?.Invoke(_activeWeapon);
            }

            return true;
        }

        /// <summary>
        /// Cambia al siguiente arma en el slot
        /// </summary>
        public void CycleToNextWeapon()
        {
            if (_ownedWeapons.Count <= 1) return;

            int currentIndex = _ownedWeapons.IndexOf(_activeWeapon);
            int nextIndex = (currentIndex + 1) % _ownedWeapons.Count;
            SetActiveWeapon(_ownedWeapons[nextIndex]);
        }
    }
}