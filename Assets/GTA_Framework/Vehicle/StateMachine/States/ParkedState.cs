using UnityEngine;
using GTAFramework.Vehicle.Components;
using GTAFramework.Vehicle.Interfaces;

namespace GTAFramework.Vehicle.States
{
    /// <summary>
    /// Estado cuando el vehículo está estacionado (sin conductor).
    /// </summary>
    public class ParkedState : VehicleState
    {
        private float _stopThreshold = 0.5f;
        public ParkedState(IVehicleContext context) : base(context) { }

        public override void Enter()
        {
            Debug.Log($"[VehicleState] {_context.Transform.name} is now PARKED");
            // Resetear todos los inputs de física
            if (_context.Physics != null)
            {
                _context.Physics.MotorInput = 0f;
                _context.Physics.SteerInput = 0f;
                _context.Physics.BrakeInput = 0f;
                _context.Physics.Handbrake = true; // Activar freno de mano
            }
        }

        public override void Update()
        {
            // Aplicar frenado gradual mientras el vehículo está en movimiento
            if (_context.Physics != null && _context.CurrentSpeed > _stopThreshold)
            {
                // Freno de mano activo para detener el vehículo
                _context.Physics.Handbrake = true;
                _context.Physics.BrakeInput = 1f;
            }
            else if (_context.CurrentSpeed <= _stopThreshold)
            {
                // Vehículo detenido, mantener freno de mano
                _context.Physics.BrakeInput = 0f;
                _context.Physics.Handbrake = true;
            }
            // Nada especial que hacer mientras está estacionado
        }

        public override void Exit()
        {
            Debug.Log($"[VehicleState] {_context.Transform.name} leaving PARKED state");
            // Desactivar freno de mano al salir del estado
            if (_context.Physics != null)
            {
                _context.Physics.Handbrake = false;
            }
        }

        public override string CheckTransitions()
        {
            // Si hay un conductor, cambiar a DrivingState
            if (_context.IsOccupied)
            {
                return VehicleStateNames.Driving;
            }

            // Permanecer en ParkedState
            return null;
        }
    }
}