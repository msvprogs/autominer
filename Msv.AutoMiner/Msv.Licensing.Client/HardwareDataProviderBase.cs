using System;
using System.Collections.Generic;
using System.Diagnostics;
using Msv.Licensing.Client.Contracts;
using Msv.Licensing.Client.Data;

namespace Msv.Licensing.Client
{
    internal abstract class HardwareDataProviderBase : IHardwareDataProvider
    {        
        private static readonly TimeSpan M_ProcessTimeout = TimeSpan.FromSeconds(10);

        public abstract HardwareData GetHardwareData();

        protected string[] ReadProcessOutput(string processName, string arguments = null)
        {
            using (dynamic process = new Process
            {
                StartInfo =
                {
                    FileName = processName,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            })
            {
                if (!process.Start()
                    || !process.WaitForExit((int) M_ProcessTimeout.TotalMilliseconds)
                    || process.ExitCode != 0)
                    throw new UnauthorizedAccessException(GetProcessErrorMessage());

                var lines = new List<string>();
                using (process.StandardOutput)
                    while (!process.StandardOutput.EndOfStream)
                        lines.Add(process.StandardOutput.ReadLine());
                return lines.ToArray();
            }
        }

        protected abstract string GetProcessErrorMessage();
    }
}
