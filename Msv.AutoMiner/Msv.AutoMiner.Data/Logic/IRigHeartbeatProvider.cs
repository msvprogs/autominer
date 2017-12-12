using System.Collections.Generic;
using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.Data.Logic
{
    public interface IRigHeartbeatProvider
    {
        Heartbeat GetLastHeartbeat(int rigId);
        Dictionary<int, Heartbeat> GetLastHeartbeats(int[] rigIds = null);
    }
}
