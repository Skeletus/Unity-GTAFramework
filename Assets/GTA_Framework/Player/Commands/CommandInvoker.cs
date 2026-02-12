using System.Collections.Generic;

namespace GTAFramework.Player.Commands
{
    /// <summary>
    /// Invocador de comandos: permite registrar y ejecutar en orden.
    /// Puedes extenderlo para grabar/replay (serializando inputs o snapshots).
    /// </summary>
    public class CommandInvoker
    {
        private readonly List<IPlayerCommand> _commands = new List<IPlayerCommand>();

        public void Register(IPlayerCommand command)
        {
            if (command != null && !_commands.Contains(command))
                _commands.Add(command);
        }

        public void Unregister(IPlayerCommand command)
        {
            if (command != null)
                _commands.Remove(command);
        }

        public void ExecuteAll(float deltaTime)
        {
            for (int i = 0; i < _commands.Count; i++)
                _commands[i].Execute(deltaTime);
        }

        public void Clear()
        {
            _commands.Clear();
        }
    }
}
