using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dapps.CqrsCore.AspNetCore
{
    public class CqrsHandlerOptions
    {
        public IEnumerable<string> HandlerAssemblyNames { get; set; }
    }
}
