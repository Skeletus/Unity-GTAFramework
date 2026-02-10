using UnityEngine;
using GTAFramework.Core.Interfaces;
using GTAFramework.GTA_Animation.Components;

namespace GTAFramework.GTA_Animation.Systems
{
    /// <summary>
    /// Sistema de animación:
    /// - Descubre CharacterAnimationAgent en escena
    /// - En Tick: construye blackboard + ejecuta módulos + setea parámetros
    /// - En LateTick: limpia flags de 1 frame (y futuro IK/root motion)
    /// </summary>
    public sealed class AnimationSystem : IGameSystem
    {
        public bool IsActive { get; set; } = true;

        private CharacterAnimationAgent[] _agents = System.Array.Empty<CharacterAnimationAgent>();

        public void Initialize()
        {
            RefreshAgents();
            Debug.Log("AnimationSystem initialized.");
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive) return;

            if (_agents == null || _agents.Length == 0)
                RefreshAgents();

            for (int i = 0; i < _agents.Length; i++)
            {
                if (_agents[i] == null) continue;
                _agents[i].Tick(deltaTime);
            }
        }

        public void LateTick(float deltaTime)
        {
            if (!IsActive) return;

            if (_agents == null || _agents.Length == 0)
                return;

            for (int i = 0; i < _agents.Length; i++)
            {
                if (_agents[i] == null) continue;
                _agents[i].LateTick(deltaTime);
            }
        }

        public void FixedTick(float fixedDeltaTime)
        {
            // Normalmente animación se maneja en Update/LateUpdate.
            // Deja Fixed para root motion/physics sync si luego lo necesitas.
        }

        public void Shutdown()
        {
            Debug.Log("AnimationSystem shutdown.");
            _agents = System.Array.Empty<CharacterAnimationAgent>();
        }

        private void RefreshAgents()
        {
            _agents = Object.FindObjectsByType<CharacterAnimationAgent>(FindObjectsSortMode.None);
        }
    }
}
