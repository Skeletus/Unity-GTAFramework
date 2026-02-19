using GTAFramework.Health.Components;
using GTAFramework.Inventory.Data;
using GTAFramework.Inventory.Interfaces;
using GTAFramework.Weapons.Components;
using GTAFramework.Weapons.Data;
using System.Collections.Generic;
using UnityEngine;

namespace GTAFramework.Inventory.Components
{
    /// <summary>
    /// Inventario simple del jugador. KISS: solo lo esencial.
    /// Implementa IPickupReceiver para recibir pickups del mundo.
    /// </summary>
    [DisallowMultipleComponent]
    public class Inventory : MonoBehaviour, IPickupReceiver
    {
        [Header("References")]
        [SerializeField] private HealthComponent _health;

        [Header("Settings")]
        [SerializeField] private int _maxSlots = 10;

        // Almacenamiento de items: ItemData -> cantidad
        private readonly Dictionary<ItemData, int> _items = new();

        // Inventario de armas
        private WeaponInventory _weaponInventory;

        // Eventos para UI, sonidos, etc.
        public event System.Action<ItemData, int> OnItemAdded;
        public event System.Action<ItemData, int> OnItemRemoved;

        // Properties
        public int SlotCount => _items.Count;
        public int MaxSlots => _maxSlots;
        public WeaponInventory Weapons => _weaponInventory;


        private void Awake()
        {
            // Auto-referencia si no está asignada
            if (_health == null)
                _health = GetComponent<HealthComponent>();

            _weaponInventory = GetComponent<WeaponInventory>();
            if (_weaponInventory == null)
                _weaponInventory = gameObject.AddComponent<WeaponInventory>();
        }

        #region IPickupReceiver Implementation

        /// <summary>
        /// ¿Puede recibir este item?
        /// </summary>
        public bool CanReceiveItem(ItemData item)
        {
            if (item == null) return false;

            // Items de efecto inmediato siempre se pueden recibir
            if (item.type == ItemType.Health || item.type == ItemType.Armor)
                return true;

            // Armas - verificar con WeaponInventory
            if (item.type == ItemType.Weapon)
            {
                // Aquí necesitarías convertir ItemData a WeaponData
                // Por ahora retornamos true para armas
                return true;
            }

            // Si ya existe, puede stackear
            if (_items.ContainsKey(item))
                return _items[item] < item.maxStack;

            // Si no existe, ¿hay espacio?
            return _items.Count < _maxSlots;
        }

        /// <summary>
        /// Recibe un item del mundo.
        /// </summary>
        public bool ReceiveItem(ItemData item, int quantity)
        {
            if (!CanReceiveItem(item)) return false;

            // Items de efecto inmediato (salud, armadura)
            if (item.type == ItemType.Health || item.type == ItemType.Armor)
            {
                ApplyItemEffect(item);
                return true;
            }

            // Items de inventario (armas, munición)
            if (_items.ContainsKey(item))
            {
                int newQty = Mathf.Min(_items[item] + quantity, item.maxStack);
                _items[item] = newQty;
            }
            else
            {
                _items[item] = Mathf.Min(quantity, item.maxStack);
            }

            OnItemAdded?.Invoke(item, quantity);
            return true;
        }

        /// <summary>
        /// Aplica efecto directo (salud/armadura).
        /// </summary>
        public void ApplyItemEffect(ItemData item)
        {
            if (_health == null || item == null) return;

            switch (item.type)
            {
                case ItemType.Health:
                    _health.Heal(item.effectValue);
                    break;
                case ItemType.Armor:
                    _health.AddArmor(item.effectValue);
                    break;
            }
        }

        #endregion

        #region Weapon API

        /// <summary>
        /// Añade un arma al inventario de armas
        /// </summary>
        public bool AddWeapon(WeaponData weapon)
        {
            return _weaponInventory?.AddWeapon(weapon) ?? false;
        }

        /// <summary>
        /// ¿Tiene esta arma?
        /// </summary>
        public bool HasWeapon(WeaponData weapon)
        {
            return _weaponInventory?.HasWeapon(weapon) ?? false;
        }

        /// <summary>
        /// Remueve un arma
        /// </summary>
        public bool RemoveWeapon(WeaponData weapon)
        {
            return _weaponInventory?.RemoveWeapon(weapon) ?? false;
        }

        /// <summary>
        /// Obtiene el arma activa
        /// </summary>
        public WeaponData GetActiveWeapon()
        {
            return _weaponInventory?.ActiveWeapon;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Obtiene la cantidad de un item en el inventario.
        /// </summary>
        public int GetItemCount(ItemData item)
        {
            return _items.TryGetValue(item, out int count) ? count : 0;
        }

        /// <summary>
        /// Remueve items del inventario.
        /// </summary>
        public bool RemoveItem(ItemData item, int quantity)
        {
            if (item == null || quantity <= 0) return false;
            if (!_items.ContainsKey(item)) return false;
            if (_items[item] < quantity) return false;

            _items[item] -= quantity;

            if (_items[item] <= 0)
            {
                _items.Remove(item);
            }

            OnItemRemoved?.Invoke(item, quantity);
            return true;
        }

        /// <summary>
        /// Verifica si tiene un item específico.
        /// </summary>
        public bool HasItem(ItemData item)
        {
            return _items.ContainsKey(item) && _items[item] > 0;
        }

        /// <summary>
        /// Obtiene todos los items (para UI).
        /// </summary>
        public IReadOnlyDictionary<ItemData, int> GetAllItems()
        {
            return _items;
        }

        #endregion
    }
}