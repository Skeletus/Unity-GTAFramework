using UnityEngine;
using GTAFramework.Core.Container;
using GTAFramework.Core.Interfaces;
using GTAFramework.Weapons.Interfaces;
using GTAFramework.Weapons.Services;
using GTAFramework.Weapons.Components;
using GTAFramework.Core.Services;

namespace GTAFramework.Weapons.Systems
{
    /// <summary>
    /// Sistema de armas:
    /// - Lee input (New Input System) via IWeaponInputHandler
    /// - Interactúa con IWeaponPicker para recoger
    /// - Cambia arma con Q/E de forma cíclica
    /// </summary>
    [AutoRegister(Priority = 14, StartActive = true)]
    public class WeaponSystem : IGameSystem
    {
        public bool IsActive { get; set; } = true;

        private IWeaponInputHandler _inputHandler;

        private IWeaponInventory _inventory;
        private IWeaponPicker _interactor;

        private bool _weaponPrevHeld;
        private bool _weaponNextHeld;

        public void Initialize()
        {
            var inputService = DIContainer.Instance.Resolve<InputService>();
            _inputHandler = new WeaponInputHandler(inputService);

            _inventory = Object.FindFirstObjectByType<WeaponInventory>();
            _interactor = Object.FindFirstObjectByType<WeaponInteractor>();

            if (_inventory == null)
                Debug.LogWarning("[WeaponSystem] No WeaponInventory found in scene.");

            if (_interactor == null)
                Debug.LogWarning("[WeaponSystem] No WeaponInteractor found in scene.");
        }

        public void Tick(float deltaTime)
        {
            if (_inputHandler == null || _inventory == null)
                return;

            HandleWeaponSwitching();
            HandlePickup();
        }

        private void HandleWeaponSwitching()
        {
            bool prevPressed = _inputHandler.IsPrevWeaponPressed;
            if (prevPressed && !_weaponPrevHeld)
                _inventory.EquipPrevious();
            _weaponPrevHeld = prevPressed;

            bool nextPressed = _inputHandler.IsNextWeaponPressed;
            if (nextPressed && !_weaponNextHeld)
                _inventory.EquipNext();
            _weaponNextHeld = nextPressed;
        }

        private void HandlePickup()
        {
            if (!_inputHandler.IsInteractPressed)
                return;

            // Solo consumimos el input si realmente recogimos algo.
            if (_interactor != null && _interactor.TryPickup(_inventory))
                _inputHandler.ConsumeInteract();
        }

        public void LateTick(float deltaTime) { }
        public void FixedTick(float fixedDeltaTime) { }
        public void Shutdown() { }
    }
}






