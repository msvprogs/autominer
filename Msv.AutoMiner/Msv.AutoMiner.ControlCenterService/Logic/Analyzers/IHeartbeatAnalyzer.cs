using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Logic.Analyzers
{
    public interface IHeartbeatAnalyzer
    {
        void Analyze(Rig rig, Heartbeat heartbeat);
    }
}
