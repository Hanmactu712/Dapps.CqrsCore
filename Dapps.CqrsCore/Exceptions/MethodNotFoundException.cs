using System;
using System.Collections.Generic;
using System.Text;

namespace Dapps.CqrsCore.Exceptions
{
    public class MethodNotFoundException : Exception
    {
        public MethodNotFoundException(Type classType, string methodName, Type paramsType) :
            base($"This class ({classType.FullName}) has no method named \"{methodName}\" that takes this parameter ({paramsType}).")
        {

        }

        public MethodNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
