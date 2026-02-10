using GTAFramework.GTA_Animation.Components;
using GTAFramework.GTA_Animation.Data;

namespace GTAFramework.GTA_Animation.Modules
{
    internal sealed class StanceModule : IAnimationModule
    {
        private CharacterAnimationAgent _agent;

        public void Initialize(CharacterAnimationAgent agent)
        {
            _agent = agent;
        }

        public void Tick(float dt, ref AnimationBlackboard bb, AnimatorDriver driver)
        {
            driver.SetBool(driver.Ids.IsCrouching, bb.isCrouching);
        }

        public void LateTick(float dt, ref AnimationBlackboard bb, AnimatorDriver driver)
        {
        }
    }
}
