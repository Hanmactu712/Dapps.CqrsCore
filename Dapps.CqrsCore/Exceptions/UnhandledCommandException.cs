using System;

namespace Dapps.CqrsCore.Exceptions;

public class UnhandledCommandException : Exception
{
    public UnhandledCommandException(string name)
        : base($"There is no handler registered for this command ({name}). One handler (and only one handler) must be registered.")
    {
    }
}
