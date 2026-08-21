namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System.Collections.Generic;
    using System.Linq;

    public class CommandRegistry
    {
        private readonly List<IObsCommand> _commands = new List<IObsCommand>();

        public void Register(IObsCommand command)
        {
            if (command != null && !this._commands.Contains(command))
            {
                this._commands.Add(command);
            }
        }

        public IEnumerable<T> GetCommands<T>() where T : IObsCommand => this._commands.OfType<T>();
    }
}
