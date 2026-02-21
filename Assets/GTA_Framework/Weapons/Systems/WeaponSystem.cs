using UnityEngine;
using GTAFramework.Core.Container;
using GTAFramework.Core.Interfaces;
using GTAFramework.Core.Services;
using GTAFramework.Weapons.Components;

namespace GTAFramework.Weapons.Systems
{
    /// <summary>
    /// Sistema de armas:
    /// - Lee input (New Input System) via InputService
    /// - Interactúa con WeaponInteractor para recoger
    /// - Cambia arma con Q/E de forma cíclica
    /// </summary>
    [AutoRegister(Priority = 14, StartActive = true)]
    public class WeaponSystem : IGameSystem
    {
        public bool IsActive { get; set; } = true;

        [Inject] private InputService _inputService;

        private WeaponInventory _inventory;
        private WeaponInteractor _interactor;

        private bool _weaponPrevHeld;
        private bool _weaponNextHeld;

        public void Initialize()
        {
            _inputService = DIContainer.Instance.Resolve<InputService>();
            _inventory = Object.FindFirstObjectByType<WeaponInventory>();
            _interactor = Object.FindFirstObjectByType<WeaponInteractor>();

            if (_inventory == null)
                Debug.LogWarning("[WeaponSystem] No WeaponInventory found in scene.");

            if (_interactor == null)
                Debug.LogWarning("[WeaponSystem] No WeaponInteractor found in scene.");
        }

        public void Tick(float deltaTime)
        {
            if (_inputService == null || _inventory == null)
                return;

            HandleWeaponSwitching();
            HandlePickup();
        }

        private void HandleWeaponSwitching()
        {
            bool prevPressed = _inputService.IsWeaponPrevPressed;
            if (prevPressed && !_weaponPrevHeld)
                _inventory.EquipPrevious();
            _weaponPrevHeld = prevPressed;

            bool nextPressed = _inputService.IsWeaponNextPressed;
            if (nextPressed && !_weaponNextHeld)
                _inventory.EquipNext();
            _weaponNextHeld = nextPressed;
        }

        private void HandlePickup()
        {
            if (!_inputService.IsInteractPressed)
                return;

            // Solo consumimos el input si realmente recogimos algo.
            if (_interactor != null && _interactor.TryPickup(_inventory))
                _inputService.IsInteractPressed = false;
        }

        public void LateTick(float deltaTime) { }
        public void FixedTick(float fixedDeltaTime) { }
        public void Shutdown() { }
    }
}
