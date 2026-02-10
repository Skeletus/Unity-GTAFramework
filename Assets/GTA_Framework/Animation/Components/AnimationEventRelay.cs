using UnityEngine;

namespace GTAFramework.GTA_Animation.Components
{
    /// <summary>
    /// Relay para Animation Events (footsteps, impacts, etc.).
    /// En GTA-like es útil para SFX/particles y gameplay cues.
    /// </summary>
    public class AnimationEventRelay : MonoBehaviour
    {
        public System.Action<string> OnEvent;

        // Llamar desde Animation Event: AnimationEventRelay.Emit("Footstep")
        public void Emit(string eventName)
        {
            OnEvent?.Invoke(eventName);
        }
    }
}
