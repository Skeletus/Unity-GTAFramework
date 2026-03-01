using System;
using UnityEngine;
using GTAFramework.GTA_Animation.Components;
using GTAFramework.GTA_Animation.Data;
using GTAFramework.Weapons.Components;
using GTAFramework.Weapons.Data;

namespace GTAFramework.GTA_Animation.Modules
{
    internal sealed class WeaponStanceModule : IAnimationModule, IDisposable
    {
        private const string PistolUpperBodyLayer = "UpperBody_Pistol";
        private const string RifleUpperBodyLayer = "UpperBody_Rifle";

        private CharacterAnimationAgent _agent;
        private WeaponInventory _weaponInventory;
        private int _pistolLayerIndex = -1;
        private int _rifleLayerIndex = -1;
        private bool _isSubscribed;

        public void Initialize(CharacterAnimationAgent agent)
        {
            _agent = agent;
            CacheLayerIndex();

            _weaponInventory = _agent.GetComponentInParent<WeaponInventory>();
            SubscribeInventory();
            HandleWeaponEquipped(_weaponInventory != null ? _weaponInventory.CurrentWeapon : null);
        }

        public void Tick(float dt, ref AnimationBlackboard bb, AnimatorDriver driver)
        {
            if (_weaponInventory != null)
                return;

            _weaponInventory = _agent.GetComponentInParent<WeaponInventory>();
            SubscribeInventory();

            if (_weaponInventory != null)
                HandleWeaponEquipped(_weaponInventory.CurrentWeapon);
        }

        public void LateTick(float dt, ref AnimationBlackboard bb, AnimatorDriver driver)
        {
        }

        public void Dispose()
        {
            if (_weaponInventory == null || !_isSubscribed)
                return;

            _weaponInventory.OnWeaponEquipped -= HandleWeaponEquipped;
            _isSubscribed = false;
        }

        private void SubscribeInventory()
        {
            if (_weaponInventory == null || _isSubscribed)
                return;

            _weaponInventory.OnWeaponEquipped += HandleWeaponEquipped;
            _isSubscribed = true;
        }

        private void CacheLayerIndex()
        {
            if (_agent == null || _agent.Driver == null)
                return;

            var animator = _agent.Driver.Animator;
            if (animator == null)
                return;

            _pistolLayerIndex = animator.GetLayerIndex(PistolUpperBodyLayer);
            _rifleLayerIndex = animator.GetLayerIndex(RifleUpperBodyLayer);
        }

        private void HandleWeaponEquipped(WeaponData weapon)
        {
            if (_agent == null || _agent.Driver == null)
                return;

            bool isPistol = weapon != null && weapon.type == WeaponType.Pistol;
            bool isRifle = weapon != null && weapon.type == WeaponType.Rifle;

            _agent.Driver.SetBool(_agent.Driver.Ids.IsPistolEquipped, isPistol);
            _agent.Driver.SetBool(_agent.Driver.Ids.IsRifleEquipped, isRifle);
            SetUpperBodyLayerWeight(isPistol, isRifle);
        }

        private void SetUpperBodyLayerWeight(bool pistolEnabled, bool rifleEnabled)
        {
            var animator = _agent.Driver.Animator;
            if (animator == null)
                return;

            if (_pistolLayerIndex < 0)
                _pistolLayerIndex = animator.GetLayerIndex(PistolUpperBodyLayer);
            if (_rifleLayerIndex < 0)
                _rifleLayerIndex = animator.GetLayerIndex(RifleUpperBodyLayer);

            if (_pistolLayerIndex >= 0)
                animator.SetLayerWeight(_pistolLayerIndex, pistolEnabled ? 1f : 0f);
            if (_rifleLayerIndex >= 0)
                animator.SetLayerWeight(_rifleLayerIndex, rifleEnabled ? 1f : 0f);
        }
    }
}
