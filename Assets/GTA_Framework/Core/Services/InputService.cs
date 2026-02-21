using UnityEngine;
using UnityEngine.InputSystem;
using GTAFramework.Core.Interfaces;

namespace GTAFramework.Core.Services
{
    public class InputService : IService
    {
        private InputActions _inputActions;

        public Vector2 MovementInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsWalkPressed { get; private set; }
        public bool IsSprintPressed { get; private set; }
        public bool IsJumpPressed { get; private set; }
        public bool IsCrouchPressed { get; private set; }
        public bool IsInteractPressed { get; set; }
        public bool IsWeaponPrevPressed { get; private set; }
        public bool IsWeaponNextPressed { get; private set; }
        public bool IsAimPressed { get; private set; }
        public bool IsShootPressed { get; private set; }

        // Estado derivado (sistemas pueden setearlo)
        public bool IsAiming { get; set; }
        public float MovementSpeedMultiplier { get; set; } = 1f;

        public void Initialize()
        {
            _inputActions = new InputActions();

            // Suscribirse a los eventos del Input System
            _inputActions.Player.Move.performed += OnMove;
            _inputActions.Player.Move.canceled += OnMove;

            _inputActions.Player.Look.performed += OnLook;
            _inputActions.Player.Look.canceled += OnLook;

            _inputActions.Player.Walk.performed += OnWalk;
            _inputActions.Player.Walk.canceled += OnWalk;

            _inputActions.Player.Sprint.performed += OnSprint;
            _inputActions.Player.Sprint.canceled += OnSprint;

            _inputActions.Player.Jump.performed += OnJump;
            _inputActions.Player.Jump.canceled += OnJump;

            _inputActions.Player.Crouch.performed += OnCrouch;
            _inputActions.Player.Crouch.canceled += OnCrouch;

            _inputActions.Player.UseVehicle.performed += OnInteract;
            _inputActions.Player.UseVehicle.canceled += OnInteract;

            _inputActions.Player.WeaponPrev.performed += OnWeaponPrev;
            _inputActions.Player.WeaponPrev.canceled += OnWeaponPrev;

            _inputActions.Player.WeaponNext.performed += OnWeaponNext;
            _inputActions.Player.WeaponNext.canceled += OnWeaponNext;

            _inputActions.Player.Aim.performed += OnAim;
            _inputActions.Player.Aim.canceled += OnAim;

            _inputActions.Player.Shoot.performed += OnShoot;
            _inputActions.Player.Shoot.canceled += OnShoot;

            _inputActions.Enable();

            Debug.Log("InputService initialized successfully.");
        }

        public void Shutdown()
        {
            if (_inputActions != null)
            {
                _inputActions.Disable();
                _inputActions.Dispose();
            }
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            LookInput = context.ReadValue<Vector2>();
        }

        private void OnWalk(InputAction.CallbackContext context)
        {
            IsWalkPressed = context.ReadValueAsButton();
        }

        private void OnSprint(InputAction.CallbackContext context)
        {
            IsSprintPressed = context.ReadValueAsButton();
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            IsJumpPressed = context.ReadValueAsButton();
        }

        private void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                IsCrouchPressed = !IsCrouchPressed;
            }
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            IsInteractPressed = context.ReadValueAsButton();
        }

        private void OnWeaponPrev(InputAction.CallbackContext context)
        {
            IsWeaponPrevPressed = context.ReadValueAsButton();
        }

        private void OnWeaponNext(InputAction.CallbackContext context)
        {
            IsWeaponNextPressed = context.ReadValueAsButton();
        }

        private void OnAim(InputAction.CallbackContext context)
        {
            IsAimPressed = context.ReadValueAsButton();
        }

        private void OnShoot(InputAction.CallbackContext context)
        {
            IsShootPressed = context.ReadValueAsButton();
        }

        public void EnableInput()
        {
            _inputActions?.Enable();
        }

        public void DisableInput()
        {
            _inputActions?.Disable();
        }
    }
}
