using GTAFramework.GTA_Animation.Components;
using GTAFramework.GTA_Animation.Data;

namespace GTAFramework.GTA_Animation.Modules
{
    // Nota: el blackboard es un struct privado en el Agent, as� que el Agent expone m�todos Tick/LateTick
    // y los m�dulos trabajan con un "ref" a ese struct mediante un alias interno.
    // Para mantener compilaci�n limpia, definimos una interfaz interna "friend-like":
    internal interface IAnimationModule
    {
        void Initialize(CharacterAnimationAgent agent);
        void Tick(float dt, ref AnimationBlackboard bb, AnimatorDriver driver);
        void LateTick(float dt, ref AnimationBlackboard bb, AnimatorDriver driver);
    }
}
