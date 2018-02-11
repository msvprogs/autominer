using System;
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
            => Array.Find((Assembly[])m_Assemblies, x => x.GetName().Name == args.Name);
    }
}
