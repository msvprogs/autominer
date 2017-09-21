using System.Diagnostics;

namespace Msv.AutoMiner.Rig.System.Contracts
{
    public interface IProcessStopper
    {
        bool StopProcess(Process process);
    }
}
