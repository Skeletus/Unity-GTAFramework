using GTAFramework.Vehicle.Components;

namespace GTAFramework.Vehicle.States
{
    /// <summary>
    /// Clase base para todos los estados del vehículo.
    /// Sigue el mismo patrón que PlayerState.
    /// </summary>
    public abstract class VehicleState
    {
        protected VehicleController _controller;

        public VehicleState(VehicleController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Llamado cuando se entra a este estado.
        /// </summary>
        public virtual void Enter() { }

        /// <summary>
        /// Llamado cada frame mientras está en este estado.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Llamado cuando se sale de este estado.
        /// </summary>
        public virtual void Exit() { }

        /// <summary>
        /// Verifica si debe cambiar a otro estado.
        /// </summary>
        /// <returns>El nuevo estado, o null para permanecer en el actual.</returns>
        public abstract VehicleState CheckTransitions();

        /// <summary>
        /// Obtiene el nombre del estado para debugging.
        /// </summary>
        public virtual string GetStateName()
        {
            return GetType().Name;
        }
    }
}