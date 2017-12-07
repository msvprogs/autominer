using System.Collections.Generic;
using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.FrontEnd.Providers
{
    public interface IRigHeartbeatProvider
    {
        Heartbeat GetLastHeartbeat(int rigId);
        Dictionary<int, Heartbeat> GetLastHeartbeats();
    }
}
