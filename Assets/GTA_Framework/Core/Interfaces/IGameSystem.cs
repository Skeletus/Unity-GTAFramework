using UnityEngine;

namespace GTAFramework.Core.Interfaces
{
    public interface IGameSystem
    {
        void Initialize();
        void Tick(float deltaTime);
        void LateTick(float deltaTime);
        void FixedTick(float fixedDeltaTime);
        void Shutdown();
        bool IsActive { get; set; }
    }
}
