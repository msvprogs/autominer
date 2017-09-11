using System.Diagnostics;

namespace Msv.AutoMiner.Service.System.Contracts
{
    public interface IProcessStopper
    {
        bool StopProcess(Process process);
    }
}
