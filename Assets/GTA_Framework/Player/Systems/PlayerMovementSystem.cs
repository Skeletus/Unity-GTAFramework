using UnityEngine;
using GTAFramework.Core.Interfaces;
using GTAFramework.Core.Services;
using GTAFramework.Player.Components;

namespace GTAFramework.Player.Systems
{
    public class PlayerMovementSystem : IGameSystem
    {
        public bool IsActive { get; set; } = true;

        private InputService _inputService;
        private PlayerController _playerController;

        private Vector3 _currentVelocity;
        private float _verticalVelocity;

        private bool _jumpRequested;

        public void Initialize()
        {
            _inputService = ServiceLocator.Instance.GetService<InputService>();
            _playerController = Object.FindFirstObjectByType<PlayerController>();

            if (_playerController == null)
                Debug.LogWarning("PlayerMovementSystem: No PlayerController found in scene!");

            Debug.Log("PlayerMovementSystem initialized.");
        }

        public void Tick(float deltaTime)
        {
            if (_playerController == null || _inputService == null)
                return;

            // Capturar salto (solo si gameplay lo permite)
            if (_inputService.IsJumpPressed && _playerController.IsGroundedStable)
                _jumpRequested = true;

            // Movimiento/rotación bloqueados durante landing (y cualquier lock)
            if (!_playerController.IsMovementLocked)
            {
                HandleMovement(deltaTime);
                HandleRotation(deltaTime);
            }
            else
            {
                // si estamos locked, cortamos velocidad horizontal
                _currentVelocity = Vector3.zero;
            }

            HandleGravity(deltaTime);

            // 3) Un solo Move final (siempre)
            Vector3 totalVelocity = _currentVelocity;
            totalVelocity.y = _verticalVelocity;

            _playerController.Velocity = totalVelocity;
            _playerController.SetVerticalSpeed(_verticalVelocity);

            _playerController.Move(totalVelocity);
        }

        public void LateTick(float deltaTime) { }

        public void FixedTick(float fixedDeltaTime)
        {
            // Evita duplicar gravedad aquí si ya lo haces en Tick (Update).
        }

        public void Shutdown()
        {
            Debug.Log("PlayerMovementSystem shutdown.");
        }

        private void HandleMovement(float deltaTime)
        {
            Vector2 input = _inputService.MovementInput;
            Vector3 moveDirection = GetMovementDirection(input);

            float targetSpeed = GetTargetSpeed();

            if (moveDirection.magnitude > 0.1f)
            {
                _currentVelocity = Vector3.Lerp(
                    _currentVelocity,
                    moveDirection * targetSpeed,
                    _playerController.MovementData.acceleration * deltaTime
                );
            }
            else
            {
                _currentVelocity = Vector3.Lerp(
                    _currentVelocity,
                    Vector3.zero,
                    _playerController.MovementData.deceleration * deltaTime
                );
            }

            //_playerController.Move(_currentVelocity);
        }

        private void HandleRotation(float deltaTime)
        {
            Vector2 input = _inputService.MovementInput;

            if (input.magnitude > 0.1f)
            {
                Vector3 moveDirection = GetMovementDirection(input);

                if (moveDirection.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    _playerController.transform.rotation = Quaternion.Slerp(
                        _playerController.transform.rotation,
                        targetRotation,
                        _playerController.MovementData.rotationSpeed * deltaTime
                    );
                }
            }
        }

        private void HandleGravity(float deltaTime)
        {
            // Saltar (solo si CanJump; en landing CanJump=false)
            if (_jumpRequested && _playerController.IsGroundedStable)
            {
                _verticalVelocity = Mathf.Sqrt(
                    _playerController.MovementData.jumpHeight * -2f * _playerController.MovementData.gravity
                );

                _jumpRequested = false;

                // Notificar al Agent (bool one-frame para IsJumping)
                _playerController.NotifyJump(_verticalVelocity);
            }

            if (_playerController.IsGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = -2f;
            }
            else
            {
                _verticalVelocity += _playerController.MovementData.gravity * deltaTime;
                _verticalVelocity = Mathf.Max(_verticalVelocity, _playerController.MovementData.maxFallSpeed);
            }

            // aplicar solo vertical (si no estás locked, Move ya fue aplicado horizontal)
            // Si estás locked, Move se ignora en PlayerController y no rompe nada.
            //_playerController.Move(new Vector3(0, _verticalVelocity, 0));
        }

        private Vector3 GetMovementDirection(Vector2 input)
        {
            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;

            if (_playerController.CameraTransform != null)
            {
                forward = _playerController.CameraTransform.forward;
                right = _playerController.CameraTransform.right;
            }

            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            return (forward * input.y + right * input.x).normalized;
        }

        private float GetTargetSpeed()
        {
            var data = _playerController.MovementData;

            _playerController.IsWalking = _inputService.IsWalkPressed;
            _playerController.IsSprinting = _inputService.IsSprintPressed;
            _playerController.IsCrouching = _inputService.IsCrouchPressed;

            if (_playerController.IsWalking)
                return data.slowWalkingSpeed;

            if (_playerController.IsCrouching)
                return data.crouchSpeed;

            if (_playerController.IsSprinting)
                return data.sprintSpeed;

            return data.runSpeed;
        }

        public void SetPlayerController(PlayerController controller)
        {
            _playerController = controller;
        }
    }
}
