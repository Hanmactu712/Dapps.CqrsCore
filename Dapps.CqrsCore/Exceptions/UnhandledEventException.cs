using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Dapps.CqrsCore.Exceptions
{
    [Serializable]
    internal class UnhandledEventException : Exception
    {
        public UnhandledEventException(string name)
            : base($"You must register at least one handler for this event ({name}).")
        {
        }

        protected UnhandledEventException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
