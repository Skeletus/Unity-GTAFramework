using UnityEngine;
using GTAFramework.Player.Components;
using GTAFramework.Core.Services;

namespace GTAFramework.Player.Commands
{
    /// <summary>
    /// Comando que maneja crouch toggle (presionar C alterna).
    /// Replica la lógica edge-triggered del sistema original.
    /// </summary>
    public class CrouchCommand : IPlayerCommand
    {
        private readonly PlayerController _controller;
        private readonly InputService _input;
        private readonly bool _debug;

        private bool _lastCrouchInputState;

        public string CommandName => "Crouch";

        public CrouchCommand(PlayerController controller, InputService inputService, bool enableDebugLogs = false)
        {
            _controller = controller;
            _input = inputService;
            _debug = enableDebugLogs;
        }

        public void Execute(float deltaTime)
        {
            if (_controller == null || _input == null)
                return;

            bool currentInputState = _input.IsCrouchPressed;

            // Solo actuar si cambió el input (edge detect)
            if (currentInputState == _lastCrouchInputState)
                return;

            _lastCrouchInputState = currentInputState;

            // Toggle crouch cuando se presiona C (dependiendo de tu InputService, esto puede ser “pressed”)
            if (_controller.IsCrouching)
            {
                // Trying to stand up
                if (_controller.TrySetCrouching(false))
                {
                    if (_debug) Debug.Log("[CrouchCommand] Standing up - space available");
                }
                else
                {
                    if (_debug) Debug.Log("[CrouchCommand] Cannot stand - obstacle above");
                }
            }
            else
            {
                // Crouching down - always allowed
                _controller.TrySetCrouching(true);
                if (_debug) Debug.Log("[CrouchCommand] Crouching down");
            }
        }

        public void ResetEdgeState()
        {
            _lastCrouchInputState = false;
        }
    }
}
