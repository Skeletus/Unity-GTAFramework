using UnityEngine;

namespace GTAFramework.Player.Components.States
{
    /// <summary>
    /// Base class for all player states in the state machine
    /// </summary>
    public abstract class PlayerState
    {
        protected PlayerController _controller;

        public PlayerState(PlayerController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Called when entering this state
        /// </summary>
        public virtual void Enter()
        {
            // Override in derived states
        }

        /// <summary>
        /// Called every frame while in this state
        /// </summary>
        public virtual void Update()
        {
            // Override in derived states
        }

        /// <summary>
        /// Called when exiting this state
        /// </summary>
        public virtual void Exit()
        {
            // Override in derived states
        }

        /// <summary>
        /// Check for state transitions
        /// </summary>
        /// <returns>The next state to transition to, or null to stay in current state</returns>
        public abstract PlayerState CheckTransitions();

        /// <summary>
        /// Get the name of this state for debugging
        /// </summary>
        public virtual string GetStateName()
        {
            return GetType().Name;
        }
    }
}