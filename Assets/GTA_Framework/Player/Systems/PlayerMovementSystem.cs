using UnityEngine;
using GTAFramework.Core.Interfaces;
using GTAFramework.Core.Services;
using GTAFramework.Player.Components;
using GTAFramework.Player.Commands;

namespace GTAFramework.Player.Systems
{
    /// <summary>
    /// Sistema de movimiento del jugador usando Command Pattern.
    /// Encapsula Move/Rotate/Jump/Crouch como comandos independientes.
    /// </summary>
    public class PlayerMovementSystem : IGameSystem
    {
        public bool IsActive { get; set; } = true;

        private InputService _inputService;
        private PlayerController _playerController;

        // Commands
        private MoveCommand _moveCommand;
        private RotateCommand _rotateCommand;
        private JumpCommand _jumpCommand;
        private CrouchCommand _crouchCommand;

        public void Initialize()
        {
            _inputService = ServiceLocator.Instance.GetService<InputService>();
            _playerController = Object.FindFirstObjectByType<PlayerController>();

            if (_playerController == null)
            {
                Debug.LogWarning("PlayerMovementSystem: No PlayerController found in scene!");
                return;
            }

            InitializeCommands();
            Debug.Log("PlayerMovementSystem initialized with Command Pattern.");
        }

        private void InitializeCommands()
        {
            _moveCommand = new MoveCommand(_playerController, _inputService);
            _rotateCommand = new RotateCommand(_playerController, _inputService);
            _jumpCommand = new JumpCommand(_playerController);
            _crouchCommand = new CrouchCommand(_playerController, _inputService, enableDebugLogs: true);
        }

        public void Tick(float deltaTime)
        {
            if (_playerController == null || _inputService == null)
                return;

            // Capturar request de salto (solo si grounded stable) tal como antes
            if (_inputService.IsJumpPressed && _playerController.IsGroundedStable)
                _jumpCommand.RequestJump();

            // Movement/rotation bloqueado durante landing (y cualquier lock)
            if (!_playerController.IsMovementLocked)
            {
                _moveCommand.Execute(deltaTime);
                _rotateCommand.Execute(deltaTime);
            }
            else
            {
                _moveCommand.ResetVelocity();
            }

            // Gravedad/salto (vertical)
            _jumpCommand.Execute(deltaTime);

            // Crouch toggle (antes de aplicar Move final)
            _crouchCommand.Execute(deltaTime);

            // Aplicar velocidad final al controller (un solo Move)
            ApplyVelocity();
        }

        private void ApplyVelocity()
        {
            Vector3 totalVelocity = _moveCommand.CurrentVelocity;
            totalVelocity.y = _jumpCommand.VerticalVelocity;

            _playerController.Velocity = totalVelocity;
            _playerController.SetVerticalSpeed(_jumpCommand.VerticalVelocity);
            _playerController.Move(totalVelocity);
        }

        public void LateTick(float deltaTime) { }

        public void FixedTick(float fixedDeltaTime)
        {
            // Intencionalmente vacío si ya aplicas todo en Tick(Update).
        }

        public void Shutdown()
        {
            Debug.Log("PlayerMovementSystem shutdown.");
        }

        public void SetPlayerController(PlayerController controller)
        {
            _playerController = controller;
            if (_playerController != null && _inputService != null)
                InitializeCommands();
        }

        // API pública útil para tests / IA / replay
        public MoveCommand MoveCommand => _moveCommand;
        public RotateCommand RotateCommand => _rotateCommand;
        public JumpCommand JumpCommand => _jumpCommand;
        public CrouchCommand CrouchCommand => _crouchCommand;
    }
}
