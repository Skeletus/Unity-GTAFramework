using UnityEngine;
using GTAFramework.Vehicle.Components;

namespace GTAFramework.Vehicle.States
{
    /// <summary>
    /// Estado cuando el vehículo está estacionado (sin conductor).
    /// </summary>
    public class ParkedState : VehicleState
    {
        private float _stopThreshold = 0.5f;
        public ParkedState(VehicleController controller) : base(controller) { }

        public override void Enter()
        {
            Debug.Log($"[VehicleState] {_controller.name} is now PARKED");
            // Resetear todos los inputs de física
            if (_controller.Physics != null)
            {
                _controller.Physics.MotorInput = 0f;
                _controller.Physics.SteerInput = 0f;
                _controller.Physics.BrakeInput = 0f;
                _controller.Physics.Handbrake = true; // Activar freno de mano
            }
        }

        public override void Update()
        {
            // Aplicar frenado gradual mientras el vehículo está en movimiento
            if (_controller.Physics != null && _controller.CurrentSpeed > _stopThreshold)
            {
                // Freno de mano activo para detener el vehículo
                _controller.Physics.Handbrake = true;
                _controller.Physics.BrakeInput = 1f;
            }
            else if (_controller.CurrentSpeed <= _stopThreshold)
            {
                // Vehículo detenido, mantener freno de mano
                _controller.Physics.BrakeInput = 0f;
                _controller.Physics.Handbrake = true;
            }
            // Nada especial que hacer mientras está estacionado
        }

        public override void Exit()
        {
            Debug.Log($"[VehicleState] {_controller.name} leaving PARKED state");
            // Desactivar freno de mano al salir del estado
            if (_controller.Physics != null)
            {
                _controller.Physics.Handbrake = false;
            }
        }

        public override VehicleState CheckTransitions()
        {
            // Si hay un conductor, cambiar a DrivingState
            if (_controller.IsOccupied)
            {
                return _controller.DrivingState;
            }

            // Permanecer en ParkedState
            return null;
        }
    }
}