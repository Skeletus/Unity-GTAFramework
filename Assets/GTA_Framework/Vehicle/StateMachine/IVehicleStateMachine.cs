using GTAFramework.Vehicle.States;
using UnityEngine;

namespace GTAFramework.Vehicle.StateMachine
{
    public interface IVehicleStateMachine
    {
        VehicleState CurrentState { get; }
        string CurrentStateName { get; }
        void RegisterState(string name, VehicleState state);
        void TransitionTo(string stateName);
        void Update();
    }
}
