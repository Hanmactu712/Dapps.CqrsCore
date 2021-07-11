using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Dapps.CqrsCore.Exceptions
{
    [Serializable]
    internal class ConcurrencyException : Exception
    {
        public ConcurrencyException(Guid aggregate)
            : base($"A concurrency violation occurred on this aggregate ({aggregate}). At least one event failed to save.")
        {
        }

        protected ConcurrencyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
