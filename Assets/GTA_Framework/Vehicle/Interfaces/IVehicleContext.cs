using GTAFramework.Vehicle.Components.Wheels;
using GTAFramework.Vehicle.Data;
using UnityEngine;

namespace GTAFramework.Vehicle.Interfaces
{
    public interface IVehicleContext
    {
        IVehiclePhysics Physics { get; }
        float CurrentSpeed { get; }
        bool IsOccupied { get; }
        bool IsGrounded { get; }
        bool IsDestroyed { get; }
        Transform Transform { get; }

        // Propiedades adicionales para DestroyedState
        Rigidbody Rigidbody { get; }
        WheelController[] Wheels { get; }
        VehicleData Data { get; }

        void Exit();
    }
}
