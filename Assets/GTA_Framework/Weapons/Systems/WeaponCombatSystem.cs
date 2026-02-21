using UnityEngine;
using GTAFramework.Core.Container;
using GTAFramework.Core.Interfaces;
using GTAFramework.Core.Services;
using GTAFramework.Player.Components;
using GTAFramework.Weapons.Components;
using GTAFramework.Weapons.Data;

namespace GTAFramework.Weapons.Systems
{
    /// <summary>
    /// Sistema de combate con armas de fuego:
    /// - Apuntado (hold)
    /// - Disparo (hold) con cadencia
    /// - Hitscan + daño
    /// </summary>
    [AutoRegister(Priority = 9, StartActive = true)]
    public class WeaponCombatSystem : IGameSystem
    {
        public bool IsActive { get; set; } = true;

        [Inject] private InputService _inputService;

        private WeaponInventory _inventory;
        private WeaponAimer _aimer;
        private WeaponShooter _shooter;
        private PlayerController _playerController;

        public void Initialize()
        {
            _inputService = DIContainer.Instance.Resolve<InputService>();

            _inventory = Object.FindFirstObjectByType<WeaponInventory>();
            _aimer = Object.FindFirstObjectByType<WeaponAimer>();
            _shooter = Object.FindFirstObjectByType<WeaponShooter>();
            _playerController = Object.FindFirstObjectByType<PlayerController>();

            if (_inventory == null)
                Debug.LogWarning("[WeaponCombatSystem] No WeaponInventory found in scene.");

            if (_aimer == null)
                Debug.LogWarning("[WeaponCombatSystem] No WeaponAimer found in scene.");

            if (_shooter == null)
                Debug.LogWarning("[WeaponCombatSystem] No WeaponShooter found in scene.");
        }

        public void Tick(float deltaTime)
        {
            if (_inputService == null || _inventory == null || _aimer == null)
                return;

            WeaponData currentWeapon = _inventory.CurrentWeapon;

            // Apuntado
            if (currentWeapon == null)
            {
                _aimer.ForceStop(_inputService);
                return;
            }

            _aimer.UpdateAiming(_inputService.IsAimPressed, currentWeapon, _inputService);

            // Disparo
            if (_aimer.IsAiming && _inputService.IsShootPressed && _shooter != null)
            {
                Transform aimOrigin = GetAimOrigin();
                _shooter.TryShoot(currentWeapon, aimOrigin, _playerController != null ? _playerController.gameObject : null);
            }
        }

        private Transform GetAimOrigin()
        {
            if (_playerController != null && _playerController.CameraTransform != null)
                return _playerController.CameraTransform;

            if (Camera.main != null)
                return Camera.main.transform;

            return null;
        }

        public void LateTick(float deltaTime) { }
        public void FixedTick(float fixedDeltaTime) { }
        public void Shutdown() { }
    }
}
