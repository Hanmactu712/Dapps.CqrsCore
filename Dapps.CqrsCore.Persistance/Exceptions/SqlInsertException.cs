using System;

namespace Dapps.CqrsCore.Persistence.Exceptions
{
    [Serializable]
    internal class SqlInsertException : Exception
    {
        public SqlInsertException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}