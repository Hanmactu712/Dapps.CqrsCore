using System;

namespace Dapps.CqrsCore.Persistence.Exceptions
{
    [Serializable]
    internal class CommandNotFoundException : Exception
    {
        public CommandNotFoundException(string message) : base(message)
        {
        }
    }
}