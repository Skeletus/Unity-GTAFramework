using UnityEngine;
using GTAFramework.GTA_Animation.Components;
using GTAFramework.GTA_Animation.Data;

namespace GTAFramework.GTA_Animation.Modules
{
    internal sealed class LocomotionModule : IAnimationModule
    {
        private CharacterAnimationAgent _agent;

        public void Initialize(CharacterAnimationAgent agent)
        {
            _agent = agent;
        }

        public void Tick(float dt, ref AnimationBlackboard bb, AnimatorDriver driver)
        {
            var cfg = _agent.Config;
            var tune = cfg.locomotion;

            // Horizontal speed
            Vector3 v = bb.velocity;
            float horizontalSpeed = new Vector3(v.x, 0f, v.z).magnitude;

            float speedParam = horizontalSpeed * tune.speedMultiplier;
            driver.SetFloatDamped(driver.Ids.Speed, speedParam, tune.speedDampTime, dt);

            // Vertical speed param (si lo usas en fall transitions)
            driver.SetFloat(driver.Ids.VerticalSpeed, bb.verticalSpeed);
        }

        public void LateTick(float dt, ref AnimationBlackboard bb, AnimatorDriver driver)
        {
            // Reservado para futuras cosas (turn in place, stride warp, etc.)
        }
    }
}
