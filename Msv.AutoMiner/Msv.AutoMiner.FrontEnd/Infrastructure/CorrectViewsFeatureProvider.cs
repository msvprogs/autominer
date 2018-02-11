using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    /// <inheritdoc />
    /// <summary>
    /// Use this class to correctly discover precompiled views assembly when loading ASP.NET Core assembly dynamically from memory.
    /// </summary>
    public class CorrectViewsFeatureProvider : ViewsFeatureProvider
    {
        private readonly Assembly m_CurrentAssembly;
        private readonly Assembly m_PrecompiledViewAssembly;

        public CorrectViewsFeatureProvider()
        {           
            m_CurrentAssembly = Assembly.GetAssembly(GetType());
            m_PrecompiledViewAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(x => x.GetName().Name == $"{m_CurrentAssembly.GetName().Name}.PrecompiledViews");
        }

        protected override IEnumerable<RazorViewAttribute> GetViewAttributes(AssemblyPart assemblyPart)
        {
            if (!m_CurrentAssembly.Equals(assemblyPart.Assembly))
                return base.GetViewAttributes(assemblyPart);

            return m_PrecompiledViewAssembly != null 
                ? m_PrecompiledViewAssembly.GetCustomAttributes<RazorViewAttribute>()
                : base.GetViewAttributes(assemblyPart);
        }
    }
}
