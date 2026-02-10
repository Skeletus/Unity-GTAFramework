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