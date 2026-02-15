using UnityEngine;
using GTAFramework.Vehicle.Components;
using GTAFramework.Vehicle.Interfaces;

namespace GTAFramework.Vehicle.States
{
    /// <summary>
    /// Estado cuando el vehículo está siendo conducido.
    /// </summary>
    public class DrivingState : VehicleState
    {
        public DrivingState(IVehicleContext context) : base(context) { }

        public override void Enter()
        {
            Debug.Log($"[VehicleState] {_context.Transform.name} is now DRIVING");
        }

        public override void Update()
        {
            // La física se maneja en VehiclePhysics.FixedUpdate()
        }

        public override void Exit()
        {
            Debug.Log($"[VehicleState] {_context.Transform.name} leaving DRIVING state");
        }

        public override string CheckTransitions()
        {
            // Si no hay conductor, cambiar a ParkedState
            if (!_context.IsOccupied)
            {
                return VehicleStateNames.Parked;
            }

            // Si el vehículo está en el aire, cambiar a AirborneState
            if (!_context.IsGrounded && _context.CurrentSpeed > 5f)
            {
                return VehicleStateNames.Airborne;
            }

            // Permanecer en DrivingState
            return null;
        }
    }
}