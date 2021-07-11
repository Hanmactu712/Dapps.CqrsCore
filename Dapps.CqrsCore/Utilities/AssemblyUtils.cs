using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dapps.CqrsCore.Utilities
{
    public static class AssemblyUtils
    {
        public static Type[] GetTypesInNameSpace(Assembly assembly, string nameSpace)
        {
            return
                !string.IsNullOrEmpty(nameSpace) ?
                assembly.GetTypes()
                    .Where(t => t.Namespace != null && t.Namespace.Contains(nameSpace, StringComparison.Ordinal))
                    .ToArray()
                : assembly.GetTypes()
                    .ToArray();
        }

        public static Type[] GetTypesDerivedFromType(Assembly assembly, Type type, string nameSpace = "")
        {
            var x = assembly.GetTypes();
            var y = assembly.GetTypes().Where(t => t.Namespace != null && t.Namespace.Contains(nameSpace, StringComparison.Ordinal));
            var z = assembly.GetTypes().Where(t => t.Namespace != null && t.Namespace.Contains(nameSpace, StringComparison.Ordinal)).Where(type.IsAssignableFrom);

            return
                !string.IsNullOrEmpty(nameSpace)
                    ? assembly.GetTypes()
                        .Where(t => t.Namespace != null && t.Namespace.Contains(nameSpace, StringComparison.Ordinal))
                        .Where(type.IsAssignableFrom)
                        .ToArray()
                    : assembly.GetTypes()
                        .Where(type.IsAssignableFrom)
                        .ToArray();
        }

        public static Type[] GetTypesFromAssemblyPath(string assemblyPath, string nameSpace)
        {
            if (File.Exists(assemblyPath))
            {
                Assembly asm = Assembly.LoadFrom(assemblyPath);

                return GetTypesInNameSpace(asm, nameSpace);
            }

            return new Type[] { };
        }

        public static Type[] GetTypesDerivedFromType(string assemblyPath, Type type)
        {
            if (File.Exists(assemblyPath))
            {
                Assembly asm = Assembly.LoadFrom(assemblyPath);

                return GetTypesDerivedFromType(asm, type);
            }

            return new Type[] { };
        }
    }
}
