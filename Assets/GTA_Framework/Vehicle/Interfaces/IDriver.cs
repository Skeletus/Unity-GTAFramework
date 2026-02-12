using UnityEngine;

namespace GTAFramework.Vehicle.Interfaces
{
    public interface IDriver
    {
        Transform Transform { get; }
        void OnVehicleEnter(IVehicle vehicle);
        void OnVehicleExit(IVehicle vehicle);
    }
}
