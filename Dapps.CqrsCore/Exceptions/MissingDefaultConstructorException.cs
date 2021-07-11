using System;
using System.Collections.Generic;
using System.Text;

namespace Dapps.CqrsCore.Exceptions
{
    class MissingDefaultConstructorException : Exception
    {
        public MissingDefaultConstructorException(Type type)
            : base($"This class has no default constructor ({type.FullName}).")
        {
        }

        //protected MissingDefaultConstructorException(SerializationInfo info, StreamingContext context) : base(info, context)
        //{
        //}
    }
}
