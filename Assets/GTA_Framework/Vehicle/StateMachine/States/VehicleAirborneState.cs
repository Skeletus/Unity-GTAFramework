using UnityEngine;
using GTAFramework.Vehicle.Components;
using GTAFramework.Vehicle.Interfaces;

namespace GTAFramework.Vehicle.States
{
    /// <summary>
    /// Estado cuando el vehículo está en el aire (saltó una rampa).
    /// No se puede controlar la dirección en este estado.
    /// </summary>
    public class VehicleAirborneState : VehicleState
    {
        public VehicleAirborneState(IVehicleContext context) : base(context) { }

        public override void Enter()
        {
            Debug.Log($"[VehicleState] {_context.Transform.name} is now AIRBORNE - No steering control!");

            // Desactivar el control de dirección
            if (_context.Physics != null)
            {
                _context.Physics.SteerInput = 0f;
            }
        }

        public override void Update()
        {
            // En el aire, no hay control de dirección
            // El vehículo sigue la física de Rigidbody (gravedad)
        }

        public override void Exit()
        {
            Debug.Log($"[VehicleState] {_context.Transform.name} landed - Steering control restored!");
        }

        public override string CheckTransitions()
        {
            // Si el vehículo aterrizó, volver a DrivingState
            if (_context.IsGrounded)
            {
                return VehicleStateNames.Driving;
            }

            // Si no hay conductor (salió mientras estaba en el aire), ir a ParkedState
            if (!_context.IsOccupied)
            {
                return VehicleStateNames.Parked;
            }

            // Permanecer en AirborneState
            return null;
        }
    }
}