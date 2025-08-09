using System;

namespace Dapps.CqrsCore.Persistence.Exceptions
{
    [Serializable]
    internal class SnapshotNotFoundException : Exception
    {
        public SnapshotNotFoundException()
        {
        }

        public SnapshotNotFoundException(string message) : base(message)
        {
        }

        public SnapshotNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}