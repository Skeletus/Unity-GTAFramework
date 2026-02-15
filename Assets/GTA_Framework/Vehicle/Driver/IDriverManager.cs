using GTAFramework.Vehicle.Interfaces;
using UnityEngine;

namespace GTAFramework.Vehicle.VehicleDriverManager
{
    public interface IDriverManager
    {
        IDriver CurrentDriver { get; }
        bool IsOccupied { get; }
        void Enter(IDriver driver);
        void Exit();
    }
}
