using System.Collections.Generic;
using UnityEngine;
using GTAFramework.Weapons.Data;

namespace GTAFramework.Weapons.Components
{
    /// <summary>
    /// Inventario simple de armas:
    /// - 1 arma por tipo (WeaponType)
    /// - Al recoger, reemplaza si ya existe ese tipo
    /// - Equipa automáticamente el arma recogida
    /// </summary>
    [DisallowMultipleComponent]
    public class WeaponInventory : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _weaponHolder;

        private readonly Dictionary<WeaponType, WeaponData> _weaponsByType = new();
        private readonly List<WeaponType> _weaponOrder = new();

        private int _currentIndex = -1;
        private GameObject _currentWeaponInstance;

        public WeaponData CurrentWeapon
        {
            get
            {
                if (_currentIndex < 0 || _currentIndex >= _weaponOrder.Count)
                    return null;

                return _weaponsByType[_weaponOrder[_currentIndex]];
            }
        }

        public IReadOnlyList<WeaponType> WeaponOrder => _weaponOrder;
        public bool HasWeapons => _weaponOrder.Count > 0;

        /// <summary>
        /// Retorna true si ya hay un arma de ese tipo en el inventario.
        /// </summary>
        public bool HasWeaponType(WeaponType type)
        {
            return _weaponsByType.ContainsKey(type);
        }

        /// <summary>
        /// Agrega o reemplaza un arma por tipo y la equipa inmediatamente.
        /// </summary>
        public bool TryAddOrReplace(WeaponData weaponData)
        {
            if (weaponData == null)
                return false;

            if (_weaponHolder == null)
            {
                Debug.LogError("[WeaponInventory] WeaponHolder no asignado. No se puede equipar el arma.");
                return false;
            }

            if (weaponData.weaponPrefab == null)
            {
                Debug.LogWarning($"[WeaponInventory] WeaponData '{weaponData.name}' no tiene prefab asignado.");
                return false;
            }

            bool existed = _weaponsByType.ContainsKey(weaponData.type);
            _weaponsByType[weaponData.type] = weaponData;

            if (!existed)
                _weaponOrder.Add(weaponData.type);

            int index = _weaponOrder.IndexOf(weaponData.type);
            EquipByIndex(index);
            return true;
        }

        /// <summary>
        /// Equipa el arma siguiente de forma cíclica.
        /// </summary>
        public void EquipNext()
        {
            if (_weaponOrder.Count == 0)
                return;

            if (_currentIndex < 0)
            {
                EquipByIndex(0);
                return;
            }

            int next = (_currentIndex + 1) % _weaponOrder.Count;
            EquipByIndex(next);
        }

        /// <summary>
        /// Equipa el arma anterior de forma cíclica.
        /// </summary>
        public void EquipPrevious()
        {
            if (_weaponOrder.Count == 0)
                return;

            if (_currentIndex < 0)
            {
                EquipByIndex(0);
                return;
            }

            int prev = (_currentIndex - 1 + _weaponOrder.Count) % _weaponOrder.Count;
            EquipByIndex(prev);
        }

        /// <summary>
        /// Equipa un arma específica por tipo (si existe).
        /// </summary>
        public bool EquipByType(WeaponType type)
        {
            if (!_weaponsByType.ContainsKey(type))
                return false;

            int index = _weaponOrder.IndexOf(type);
            if (index < 0)
                return false;

            EquipByIndex(index);
            return true;
        }

        private void EquipByIndex(int index)
        {
            if (_weaponOrder.Count == 0)
                return;

            _currentIndex = Mathf.Clamp(index, 0, _weaponOrder.Count - 1);

            WeaponType type = _weaponOrder[_currentIndex];
            WeaponData data = _weaponsByType[type];

            SpawnWeaponModel(data);
        }

        private void SpawnWeaponModel(WeaponData data)
        {
            if (data == null || data.weaponPrefab == null)
                return;

            if (_currentWeaponInstance != null)
                Destroy(_currentWeaponInstance);

            _currentWeaponInstance = Instantiate(data.weaponPrefab, _weaponHolder);

            if (data.useCustomPose)
            {
                _currentWeaponInstance.transform.localPosition = data.localPosition;
                _currentWeaponInstance.transform.localRotation = Quaternion.Euler(data.localEulerAngles);
                _currentWeaponInstance.transform.localScale = data.localScale;
            }
            else
            {
                _currentWeaponInstance.transform.localPosition = Vector3.zero;
                _currentWeaponInstance.transform.localRotation = Quaternion.identity;
                _currentWeaponInstance.transform.localScale = Vector3.one;
            }
        }

        private void OnDestroy()
        {
            if (_currentWeaponInstance != null)
                Destroy(_currentWeaponInstance);
        }
    }
}
