using UnityEngine;

namespace GTAFramework.Player.Commands
{
    /// <summary>
    /// Interfaz base para todos los comandos del jugador.
    /// Encapsula una acción como un objeto (Command Pattern).
    /// </summary>
    public interface IPlayerCommand
    {
        /// <summary>Nombre del comando (debug).</summary>
        string CommandName { get; }

        /// <summary>Ejecuta el comando.</summary>
        void Execute(float deltaTime);
    }

    /// <summary>
    /// Interfaz opcional para comandos con Undo.
    /// </summary>
    public interface IUndoableCommand : IPlayerCommand
    {
        void Undo();
    }
}
