using System;

namespace Dapps.CqrsCore.Persistence.Exceptions
{
    internal class DbEntityException : Exception
    {
        public DbEntityException(Exception inner) : base(null, inner) { }

        public DbEntityException(string s, Exception exception) : base(s, exception) { }
    }
}
