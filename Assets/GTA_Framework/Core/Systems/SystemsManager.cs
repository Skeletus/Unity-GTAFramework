using System.Collections.Generic;
using UnityEngine;
using GTAFramework.Core.Interfaces;

namespace GTAFramework.Core.Systems
{
    public class SystemsManager
    {
        private readonly List<IGameSystem> _systems = new List<IGameSystem>();

        public void RegisterSystem(IGameSystem system)
        {
            if (!_systems.Contains(system))
            {
                _systems.Add(system);
                system.Initialize();
                Debug.Log($"System {system.GetType().Name} registered.");
            }
        }

        public void UnregisterSystem(IGameSystem system)
        {
            if (_systems.Contains(system))
            {
                system.Shutdown();
                _systems.Remove(system);
            }
        }

        public T GetSystem<T>() where T : class, IGameSystem
        {
            foreach (var system in _systems)
            {
                if (system is T typedSystem)
                    return typedSystem;
            }
            return null;
        }

        public void Tick(float deltaTime)
        {
            foreach (var system in _systems)
            {
                if (system.IsActive)
                    system.Tick(deltaTime);
            }
        }

        public void LateTick(float deltaTime)
        {
            foreach (var system in _systems)
            {
                if (system.IsActive)
                    system.LateTick(deltaTime);
            }
        }

        public void FixedTick(float fixedDeltaTime)
        {
            foreach (var system in _systems)
            {
                if (system.IsActive)
                    system.FixedTick(fixedDeltaTime);
            }
        }

        public void ShutdownAll()
        {
            foreach (var system in _systems)
            {
                system.Shutdown();
            }
            _systems.Clear();
        }
    }
}