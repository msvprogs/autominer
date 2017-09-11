using System;
using System.Collections.Generic;
using Msv.AutoMiner.Service.System.Windows;

namespace Msv.AutoMiner.Service.System.Unix
{
    public class UnixEnvironmentVariableCreator : StandardEnvironmentVariableCreator
    {
        private readonly string m_CudaLibraryPath;

        public UnixEnvironmentVariableCreator(string cudaLibraryPath)
        {
            m_CudaLibraryPath = cudaLibraryPath;
        }

        public override IDictionary<string, string> Create()
        {
            var variables = base.Create();
            if (!string.IsNullOrEmpty(m_CudaLibraryPath))
                variables.Add("LD_LIBRARY_PATH", Environment.ExpandEnvironmentVariables($"%LD_LIBRARY_PATH%:{m_CudaLibraryPath}"));
            return variables;
        }
    }
}
