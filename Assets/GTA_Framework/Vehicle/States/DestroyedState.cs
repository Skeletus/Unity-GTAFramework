using UnityEngine;
using GTAFramework.Vehicle.Components;
using Unity.VisualScripting;

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

        public DestroyedState(VehicleController controller) : base(controller) { }

        public override void Enter()
        {
            Debug.Log($"[VehicleState] {_controller.name} is now DESTROYED!");

            // Expulsar al conductor si hay uno
            if (_controller.IsOccupied && !_hasExpelledDriver)
            {
                ExpelDriver();
            }

            // Desactivar física del vehículo
            if (_controller.Physics != null)
            {
                _controller.Physics.ResetInputs();
                _controller.Physics.MotorInput = 0f;
                _controller.Physics.SteerInput = 0f;
                _controller.Physics.BrakeInput = 1f; // Freno total
                _controller.Physics.Handbrake = true;
            }

            // Detener el vehículo
            if (_controller.Rigidbody != null)
            {
                _controller.Rigidbody.linearVelocity = Vector3.zero;
                _controller.Rigidbody.angularVelocity = Vector3.zero;
                // Aumentar drag para que no se mueva
                _controller.Rigidbody.linearDamping = 10f;
                _controller.Rigidbody.angularDamping = 10f;
            }

            foreach (var wheel in _controller.Wheels)
            {
                wheel.MotorTorque = 0f;
                wheel.BrakeTorque = 10000f; // Freno máximo
            }

            // TODO: Activar efectos visuales de destrucción
            // - Partículas de humo
            // - Cambio de material (quemado)
            // - Sonido de explosión

            _smokeTimer = 0f;
        }

        public override void Update()
        {
            // Mantener el vehículo detenido
            if (_controller.Rigidbody != null)
            {
                // Si por alguna razón sigue moviéndose, detenerlo
                if (_controller.Rigidbody.linearVelocity.magnitude > 0.1f)
                {
                    _controller.Rigidbody.linearVelocity *= 0.9f; // Reducir velocidad gradualmente
                }
                if (_controller.Rigidbody.angularVelocity.magnitude > 0.1f)
                {
                    _controller.Rigidbody.angularVelocity *= 0.9f;
                }
            }

            // Efecto de humo periódico (placeholder)
            _smokeTimer += Time.deltaTime;
            if (_smokeTimer >= _smokeInterval)
            {
                _smokeTimer = 0f;
                Debug.Log($"[DestroyedState] {_controller.name} is smoking...");
                // TODO: Instanciar partículas de humo
            }
        }

        public override void Exit()
        {
            Debug.Log($"[VehicleState] {_controller.name} has been REPAIRED!");

            if (_controller.Rigidbody != null && _controller.Data != null)
            {
                _controller.Rigidbody.linearDamping = 0f; // Valor normal
                _controller.Rigidbody.angularDamping = 0.05f; // Valor normal
            }

            // TODO: Desactivar efectos visuales de destrucción
            _hasExpelledDriver = false;
        }

        public override VehicleState CheckTransitions()
        {
            
            // Si el vehículo ya no está destruido (fue reparado), ir a ParkedState
            if (!_controller.IsDestroyed)
            {
                return _controller.ParkedState;
            }
            

            // Permanecer en DestroyedState
            return null;
        }

        /// <summary>
        /// Expulsa al conductor del vehículo destruido.
        /// </summary>
        private void ExpelDriver()
        {
            _hasExpelledDriver = true;

            // Calcular posición de expulsión (alejada del vehículo)
            Vector3 expelPosition = _controller.transform.position +
                                    _controller.transform.right * 3f +
                                    Vector3.up * 0.5f;

            // Forzar salida del conductor
            _controller.Exit();

            Debug.Log($"[DestroyedState] Driver expelled from destroyed vehicle {_controller.name}");

            // TODO: Aplicar daño al conductor por la explosión
            //_controller.CurrentDriver?.ApplyDamage(20f);
        }
    }
}