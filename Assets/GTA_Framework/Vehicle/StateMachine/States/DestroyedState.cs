using UnityEngine;
using GTAFramework.Vehicle.Components;
using Unity.VisualScripting;
using GTAFramework.Vehicle.Interfaces;

namespace GTAFramework.Vehicle.States
{
    /// <summary>
    /// Estado cuando el vehículo está destruido.
    /// El vehículo es inoperable, humea, y el conductor es expulsado.
    /// </summary>
    public class DestroyedState : VehicleState
    {
        private bool _hasExpelledDriver;
        private float _smokeTimer;
        private float _smokeInterval = 2f;

        public DestroyedState(IVehicleContext context) : base(context) { }

        public override void Enter()
        {
            Debug.Log($"[VehicleState] {_context.Transform.name} is now DESTROYED!");

            ExpelDriverIfNeeded();
            DisableVehiclePhysics();
            StopWheels();
            ResetSmokeTimer();
        }

        public override void Update()
        {
            GraduallyStopVehicle();
            UpdateSmokeEffect();
        }

        public override void Exit()
        {
            Debug.Log($"[VehicleState] {_context.Transform.name} has been REPAIRED!");

            ResetRigidbodyDamping();
            _hasExpelledDriver = false;
        }

        public override string CheckTransitions()
        {
            // Si el vehículo ya no está destruido (fue reparado), ir a ParkedState
            if (!_context.IsDestroyed)
            {
                return VehicleStateNames.Parked;
            }

            // Permanecer en DestroyedState
            return null;
        }

        // ========== MÉTODOS PRIVADOS ==========

        private void ExpelDriverIfNeeded()
        {
            if (_context.IsOccupied && !_hasExpelledDriver)
            {
                _hasExpelledDriver = true;
                _context.Exit();
                Debug.Log($"[DestroyedState] Driver expelled from {_context.Transform.name}");
            }
        }

        private void DisableVehiclePhysics()
        {
            if (_context.Physics == null) return;

            _context.Physics.ResetInputs();
            _context.Physics.BrakeInput = 1f;
            _context.Physics.Handbrake = true;

            if (_context.Rigidbody != null)
            {
                _context.Rigidbody.linearVelocity = Vector3.zero;
                _context.Rigidbody.angularVelocity = Vector3.zero;
                _context.Rigidbody.linearDamping = 10f;
                _context.Rigidbody.angularDamping = 10f;
            }
        }

        private void StopWheels()
        {
            if (_context.Wheels == null) return;

            foreach (var wheel in _context.Wheels)
            {
                wheel.MotorTorque = 0f;
                wheel.BrakeTorque = 10000f;
            }
        }

        private void GraduallyStopVehicle()
        {
            var rb = _context.Rigidbody;
            if (rb == null) return;

            if (rb.linearVelocity.magnitude > 0.1f)
                rb.linearVelocity *= 0.9f;

            if (rb.angularVelocity.magnitude > 0.1f)
                rb.angularVelocity *= 0.9f;
        }

        private void UpdateSmokeEffect()
        {
            _smokeTimer += Time.deltaTime;
            if (_smokeTimer >= _smokeInterval)
            {
                _smokeTimer = 0f;
                Debug.Log($"[DestroyedState] {_context.Transform.name} is smoking...");
                // TODO: Instanciar partículas de humo
            }
        }

        private void ResetRigidbodyDamping()
        {
            if (_context.Rigidbody != null)
            {
                _context.Rigidbody.linearDamping = 0f;
                _context.Rigidbody.angularDamping = 0.05f;
            }
        }

        private void ResetSmokeTimer() => _smokeTimer = 0f;
    }
}