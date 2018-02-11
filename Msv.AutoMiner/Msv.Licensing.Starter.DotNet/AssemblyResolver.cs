using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Msv.Licensing.Starter.DotNet
{
    public class AssemblyResolver : IDisposable
    {
        private readonly dynamic m_Assemblies;

        public AssemblyResolver(dynamic assemblies)
        {
            m_Assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        public void Dispose()
            => AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;

        private Assembly OnAssemblyResolve(dynamic sender, dynamic args)
            => Array.Find(((IEnumerable<dynamic>)m_Assemblies).Cast<Assembly>().ToArray(), x => x.GetName().Name == args.Name);
    }
}
