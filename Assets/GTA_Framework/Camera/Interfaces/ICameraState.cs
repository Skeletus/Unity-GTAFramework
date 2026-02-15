using GTAFramework.GTACamera.Data;
using UnityEngine;

namespace GTAFramework.GTACamera.Interfaces
{
    public interface ICameraState
    {
        string StateName { get; }
        void Enter(StateTransitionContext context);
        void Exit();
        void Update(float deltaTime);
        void HandleRotation(Vector2 lookInput);
        Vector3 GetDesiredPosition();
        Quaternion GetDesiredRotation();
    }
}