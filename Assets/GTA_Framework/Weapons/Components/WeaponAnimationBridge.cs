using UnityEngine;
using GTAFramework.GTA_Animation.Components;
using GTAFramework.Weapons.Data;

namespace GTAFramework.Weapons.Components
{
    /// <summary>
    /// Puente simple entre WeaponInventory y Animator.
    /// Cambia el idle y el layer segun el tipo de arma equipada.
    /// </summary>
    [DisallowMultipleComponent]
    public class WeaponAnimationBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WeaponInventory _weaponInventory;
        [SerializeField] private CharacterAnimationAgent _animationAgent;

        [Header("Animator Layer")]
        [SerializeField] private string _pistolUpperBodyLayer = "UpperBody_Pistol";
        [SerializeField] private bool _enableUpperBodyLayer = true;

        private int _pistolLayerIndex = -1;

        private void Awake()
        {
            if (_weaponInventory == null)
                _weaponInventory = GetComponentInParent<WeaponInventory>();

            if (_animationAgent == null)
                _animationAgent = GetComponentInParent<CharacterAnimationAgent>();
        }

        private void OnEnable()
        {
            if (_weaponInventory != null)
                _weaponInventory.OnWeaponEquipped += HandleWeaponEquipped;
        }

        private void OnDisable()
        {
            if (_weaponInventory != null)
                _weaponInventory.OnWeaponEquipped -= HandleWeaponEquipped;
        }

        private void Start()
        {
            CacheLayerIndex();

            if (_weaponInventory != null)
                HandleWeaponEquipped(_weaponInventory.CurrentWeapon);
        }

        private void CacheLayerIndex()
        {
            if (_animationAgent == null || _animationAgent.Driver == null)
                return;

            var animator = _animationAgent.Driver.Animator;
            if (animator == null)
                return;

            if (!string.IsNullOrWhiteSpace(_pistolUpperBodyLayer))
                _pistolLayerIndex = animator.GetLayerIndex(_pistolUpperBodyLayer);
        }

        private void HandleWeaponEquipped(WeaponData weapon)
        {
            if (_animationAgent == null || _animationAgent.Driver == null)
                return;

            bool isPistol = weapon != null && weapon.type == WeaponType.Pistol;

            _animationAgent.Driver.SetBool(_animationAgent.Driver.Ids.IsPistolEquipped, isPistol);
            SetUpperBodyLayerWeight(isPistol);
        }

        private void SetUpperBodyLayerWeight(bool enable)
        {
            if (!_enableUpperBodyLayer)
                return;

            var animator = _animationAgent.Driver.Animator;
            if (animator == null)
                return;

            if (_pistolLayerIndex < 0)
            {
                _pistolLayerIndex = animator.GetLayerIndex(_pistolUpperBodyLayer);
                if (_pistolLayerIndex < 0)
                    return;
            }

            animator.SetLayerWeight(_pistolLayerIndex, enable ? 1f : 0f);
        }
    }
}
