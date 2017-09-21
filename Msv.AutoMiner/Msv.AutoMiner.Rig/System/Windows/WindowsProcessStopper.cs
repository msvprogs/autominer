using System.Diagnostics;
using Msv.AutoMiner.Rig.System.Contracts;

namespace Msv.AutoMiner.Rig.System.Windows
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