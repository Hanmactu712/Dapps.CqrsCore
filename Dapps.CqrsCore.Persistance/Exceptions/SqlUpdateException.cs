using System;

namespace Dapps.CqrsCore.Persistence.Exceptions
{
    [Serializable]
    internal class SqlUpdateException : Exception
    {
        public SqlUpdateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}