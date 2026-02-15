using GTAFramework.Vehicle.States;
using System.Collections.Generic;
using UnityEngine;

namespace GTAFramework.Vehicle.StateMachine
{
    public class VehicleStateMachine : IVehicleStateMachine
    {
        private readonly Dictionary<string, VehicleState> _states = new();
        private VehicleState _currentState;

        public VehicleState CurrentState => _currentState;
        public string CurrentStateName => _currentState?.GetStateName() ?? "";

        public void RegisterState(string name, VehicleState state)
        {
            _states[name] = state;
        }

        public void TransitionTo(string stateName)
        {
            if (!_states.TryGetValue(stateName, out var newState)) return;
            if (newState == _currentState) return;

            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }

        public void Update()
        {
            _currentState?.Update();

            var nextStateName = _currentState?.CheckTransitions();
            if (!string.IsNullOrEmpty(nextStateName))
            {
                TransitionTo(nextStateName);
            }
        }
    }
}