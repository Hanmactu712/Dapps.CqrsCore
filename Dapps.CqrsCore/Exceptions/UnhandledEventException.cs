using System;

namespace Dapps.CqrsCore.Exceptions;

[Serializable]
internal class UnhandledEventException : Exception
{
    public UnhandledEventException(string name)
        : base($"You must register at least one handler for this event ({name}).")
    {
    }
}
