using UnityEngine;
using GTAFramework.Vehicle.Components;

namespace GTAFramework.Vehicle.States
{
    /// <summary>
    /// Estado cuando el vehículo está siendo conducido.
    /// </summary>
    public class DrivingState : VehicleState
    {
        public DrivingState(VehicleController controller) : base(controller) { }

        public override void Enter()
        {
            Debug.Log($"[VehicleState] {_controller.name} is now DRIVING");
        }

        public override void Update()
        {
            // La física se maneja en VehiclePhysics.FixedUpdate()
        }

        public override void Exit()
        {
            Debug.Log($"[VehicleState] {_controller.name} leaving DRIVING state");
        }

        public override VehicleState CheckTransitions()
        {
            // Si no hay conductor, cambiar a ParkedState
            if (!_controller.IsOccupied)
            {
                return _controller.ParkedState;
            }

            // Si el vehículo está en el aire, cambiar a AirborneState
            if (!_controller.IsGrounded && _controller.CurrentSpeed > 5f)
            {
                return _controller.AirborneState;
            }

            // Permanecer en DrivingState
            return null;
        }
    }
}