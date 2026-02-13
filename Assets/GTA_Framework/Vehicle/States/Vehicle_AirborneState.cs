using UnityEngine;
using GTAFramework.Vehicle.Components;

namespace GTAFramework.Vehicle.States
{
    /// <summary>
    /// Estado cuando el vehículo está en el aire (saltó una rampa).
    /// No se puede controlar la dirección en este estado.
    /// </summary>
    public class Vehicle_AirborneState : VehicleState
    {
        public Vehicle_AirborneState(VehicleController controller) : base(controller) { }

        public override void Enter()
        {
            Debug.Log($"[VehicleState] {_controller.name} is now AIRBORNE - No steering control!");

            // Desactivar el control de dirección
            if (_controller.Physics != null)
            {
                _controller.Physics.SteerInput = 0f;
            }
        }

        public override void Update()
        {
            // En el aire, no hay control de dirección
            // El vehículo sigue la física de Rigidbody (gravedad)
        }

        public override void Exit()
        {
            Debug.Log($"[VehicleState] {_controller.name} landed - Steering control restored!");
        }

        public override VehicleState CheckTransitions()
        {
            // Si el vehículo aterrizó, volver a DrivingState
            if (_controller.IsGrounded)
            {
                return _controller.DrivingState;
            }

            // Si no hay conductor (salió mientras estaba en el aire), ir a ParkedState
            if (!_controller.IsOccupied)
            {
                return _controller.ParkedState;
            }

            // Permanecer en AirborneState
            return null;
        }
    }
}