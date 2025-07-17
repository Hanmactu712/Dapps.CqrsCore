using System;
using System.Runtime.Serialization;

namespace Dapps.CqrsCore.Exceptions;

[Serializable]
internal class ConcurrencyException : Exception
{
    public ConcurrencyException(Guid aggregate)
        : base($"A concurrency violation occurred on this aggregate ({aggregate}). At least one event failed to save.")
    {
    }
}
