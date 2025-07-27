using System.Collections.Generic;
using System.Reflection;

namespace Dapps.CqrsCore.AspNetCore
{
    public class CqrsHandlerOptions
    {
        /// <summary>
        /// List of assembly names which contains the CQRS handlers.
        /// </summary>
        public IEnumerable<string> HandlerAssemblyNames { get; set; } = new List<string>();

        /// <summary>
        /// List of assemblies which contains the CQRS handlers.
        /// </summary>
        public IEnumerable<Assembly> HandlerAssemblies { get; set; } = new List<Assembly>();
    }
}
