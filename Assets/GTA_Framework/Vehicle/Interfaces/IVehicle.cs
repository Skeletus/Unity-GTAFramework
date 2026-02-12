using UnityEngine;

namespace GTAFramework.Vehicle.Interfaces
{
    public interface IVehicle
    {
        bool IsOccupied { get; }
        Transform Transform { get; }

        void Enter(IDriver driver);
        void Exit();
    }
}
