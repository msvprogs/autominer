using System.Diagnostics;
using Msv.AutoMiner.Service.System.Contracts;

namespace Msv.AutoMiner.Service.System.Windows
{
    public class WindowsProcessStopper : IProcessStopper
    {
        public bool StopProcess(Process process)
        {
            //Windows is a beast.
            process.Kill();
            return true;
        }
    }
}