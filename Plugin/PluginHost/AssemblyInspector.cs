using Plugin.Interfaces;
using System;
using System.Reflection;

namespace Plugin.PluginHost
{
    internal class AssemblyInspector : MarshalByRefObject, IAssemblyInspector
    {
        public string GetFullName(byte[] assemblyBytes)
        {
            return Assembly.ReflectionOnlyLoad(assemblyBytes).FullName; 
        }
    }
}
