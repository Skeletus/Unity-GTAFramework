using UnityEngine;
using GTAFramework.Core.Services;
using GTAFramework.GTACamera.Components;
using GTAFramework.Weapons.Data;

namespace GTAFramework.Weapons.Components
{
    /// <summary>
    /// Maneja el estado de apuntado: camara, multiplicador de movimiento y estado global.
    /// </summary>
    [DisallowMultipleComponent]
    public class WeaponAimer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ThirdPersonCamera _thirdPersonCamera;

        public bool IsAiming { get; private set; }

        private void Awake()
        {
            if (_thirdPersonCamera == null)
                _thirdPersonCamera = FindFirstObjectByType<ThirdPersonCamera>();
        }

        /// <summary>
        /// Actualiza el estado de apuntado segun input y arma equipada.
        /// </summary>
        public void UpdateAiming(bool aimInput, WeaponData weapon, InputService input)
        {
            bool canAim = weapon != null && weapon.isFirearm;
            bool shouldAim = canAim && aimInput;

            if (IsAiming != shouldAim)
            {
                IsAiming = shouldAim;
                SetCameraState(IsAiming);
            }

            if (input != null)
            {
                input.IsAiming = IsAiming;
                input.MovementSpeedMultiplier = IsAiming && weapon != null
                    ? Mathf.Clamp(weapon.aimMoveSpeedMultiplier, 0.1f, 1f)
                    : 1f;
            }
        }

        /// <summary>
        /// Fuerza a salir del apuntado (por ejemplo, si no hay arma equipada).
        /// </summary>
        public void ForceStop(InputService input)
        {
            if (!IsAiming && (input == null || input.MovementSpeedMultiplier == 1f))
                return;

            IsAiming = false;
            SetCameraState(false);

            if (input != null)
            {
                input.IsAiming = false;
                input.MovementSpeedMultiplier = 1f;
            }
        }

        private void SetCameraState(bool aiming)
        {
            if (_thirdPersonCamera == null)
                return;

            _thirdPersonCamera.SetState(aiming ? "Aiming" : "Normal");
        }
    }
}
