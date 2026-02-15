using UnityEngine;

namespace GTAFramework.Vehicle.Components.VehicleCollision
{
    public interface IVehicleCollisionHandler
    {
        void HandleCollision(Collision collision);
    }
}