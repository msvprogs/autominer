using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.ControlCenterService.Logic.Analyzers
{
    public interface IHeartbeatAnalyzer
    {
        void Analyze(int rigId, Heartbeat heartbeat);
    }
}
