using GTAFramework.Weapons.Data;
using System.Collections.Generic;
using UnityEngine;

namespace GTAFramework.Weapons.Components
{
    /// <summary>
    /// Gestiona el inventario de armas del jugador.
    /// Múltiples slots, cada uno con un tipo de arma específico.
    /// </summary>
    public class WeaponInventory : MonoBehaviour
    {
        [Header("Weapon Slots Configuration")]
        [SerializeField] private List<WeaponType> _slotTypes = new();

        // Storage
        private readonly Dictionary<WeaponType, WeaponSlot> _weaponSlots = new();

        // Estado
        private WeaponSlot _currentSlot;

        // Eventos
        public event System.Action<WeaponData> OnActiveWeaponChanged;
        public event System.Action<WeaponSlot> OnSlotChanged;

        // Properties
        public WeaponData ActiveWeapon => _currentSlot?.ActiveWeapon;
        public WeaponSlot CurrentSlot => _currentSlot;
        public int SlotCount => _weaponSlots.Count;

        private void Awake()
        {
            InitializeSlots();
        }

        private void InitializeSlots()
        {
            _weaponSlots.Clear();

            foreach (var type in _slotTypes)
            {
                if (type != null && !_weaponSlots.ContainsKey(type))
                {
                    var slot = new WeaponSlot(type);
                    slot.OnActiveWeaponChanged += OnSlotActiveWeaponChanged;
                    _weaponSlots[type] = slot;
                }
            }
        }

        private void OnSlotActiveWeaponChanged(WeaponData weapon)
        {
            OnActiveWeaponChanged?.Invoke(weapon);
        }

        /// <summary>
        /// Obtiene un slot por su tipo
        /// </summary>
        public WeaponSlot GetSlot(WeaponType type)
        {
            return _weaponSlots.TryGetValue(type, out var slot) ? slot : null;
        }

        /// <summary>
        /// Obtiene todos los slots
        /// </summary>
        public IReadOnlyDictionary<WeaponType, WeaponSlot> GetAllSlots()
        {
            return _weaponSlots;
        }

        /// <summary>
        /// Añade un arma al inventario
        /// </summary>
        public bool AddWeapon(WeaponData weapon)
        {
            if (weapon == null || weapon.weaponType == null) return false;

            var slot = GetSlot(weapon.weaponType);
            if (slot == null)
            {
                // Crear slot dinámicamente si no existe
                slot = new WeaponSlot(weapon.weaponType);
                slot.OnActiveWeaponChanged += OnSlotActiveWeaponChanged;
                _weaponSlots[weapon.weaponType] = slot;
            }

            if (!slot.AddWeapon(weapon)) return false;

            OnSlotChanged?.Invoke(slot);
            return true;
        }

        /// <summary>
        /// ¿Tiene esta arma?
        /// </summary>
        public bool HasWeapon(WeaponData weapon)
        {
            var slot = GetSlot(weapon.weaponType);
            return slot?.HasWeapon(weapon) ?? false;
        }

        /// <summary>
        /// Remueve un arma
        /// </summary>
        public bool RemoveWeapon(WeaponData weapon)
        {
            var slot = GetSlot(weapon.weaponType);
            if (slot == null) return false;

            if (!slot.RemoveWeapon(weapon)) return false;

            OnSlotChanged?.Invoke(slot);
            return true;
        }

        /// <summary>
        /// Establece el slot activo por tipo
        /// </summary>
        public bool SetActiveSlot(WeaponType type)
        {
            var slot = GetSlot(type);
            if (slot == null) return false;

            _currentSlot = slot;
            OnActiveWeaponChanged?.Invoke(slot.ActiveWeapon);
            return true;
        }

        /// <summary>
        /// Cambia al siguiente slot
        /// </summary>
        public void CycleToNextSlot()
        {
            if (_weaponSlots.Count == 0) return;

            var slots = new List<WeaponSlot>(_weaponSlots.Values);
            int currentIndex = slots.IndexOf(_currentSlot);
            int nextIndex = (currentIndex + 1) % slots.Count;

            _currentSlot = slots[nextIndex];
            OnActiveWeaponChanged?.Invoke(_currentSlot.ActiveWeapon);
        }

        /// <summary>
        /// Cambia al arma siguiente en el slot actual
        /// </summary>
        public void CycleWeaponInCurrentSlot()
        {
            _currentSlot?.CycleToNextWeapon();
        }
    }
}