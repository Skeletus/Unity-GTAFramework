namespace GTAFramework.Vehicle.Commands
{
    /// <summary>
    /// Interfaz base para todos los comandos de vehículo.
    /// Sigue el mismo patrón que IPlayerCommand.
    /// </summary>
    public interface IVehicleCommand
    {
        string CommandName { get; }
        void Execute(float deltaTime);
    }
}