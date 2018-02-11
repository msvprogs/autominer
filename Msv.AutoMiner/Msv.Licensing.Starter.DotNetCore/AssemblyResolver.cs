using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Msv.Licensing.Starter.DotNetCore
{
    public class AssemblyResolver : IDisposable
    {
        private readonly dynamic m_Assemblies;

        public AssemblyResolver(dynamic assemblies)
        {
            m_Assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
            AssemblyLoadContext.Default.Resolving += OnAssemblyResolve;
        }

        public void Dispose()
            => AssemblyLoadContext.Default.Resolving -= OnAssemblyResolve;

        private Assembly OnAssemblyResolve(dynamic assemblyLoadContext, dynamic assemblyName)
            => Array.Find((Assembly[])m_Assemblies, x => x.GetName().FullName == assemblyName.ToString());
    }
}
