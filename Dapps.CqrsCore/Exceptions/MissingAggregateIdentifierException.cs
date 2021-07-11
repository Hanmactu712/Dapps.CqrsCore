using System;
using System.Collections.Generic;
using System.Text;

namespace Dapps.CqrsCore.Exceptions
{
    internal class MissingAggregateIdentifierException : Exception
    {
        public MissingAggregateIdentifierException(Type aggregateType, Type eventType)
            : base($"The aggregate identifier is missing from both the aggregate instance ({aggregateType.FullName}) and the event instance ({eventType.FullName}).")
        {
        }

        //protected MissingAggregateIdentifierException(SerializationInfo info, StreamingContext context) : base(info, context)
        //{
        //}
    }
}
