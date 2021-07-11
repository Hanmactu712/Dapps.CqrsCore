using System;
using System.Runtime.Serialization;

namespace Dapps.CqrsCore.Persistence.Exceptions
{
    [Serializable]
    internal class SqlUpdateException : Exception
    {
        public SqlUpdateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SqlUpdateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}