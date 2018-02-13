using System.Collections.Generic;
using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.Data.Logic.Contracts
{
    public interface IRigHeartbeatProvider
    {
        (Heartbeat heartbeat, RigHeartbeat entity) GetLastHeartbeat(int rigId);
        Dictionary<int, (Heartbeat heartbeat, RigHeartbeat entity)> GetLastHeartbeats(int[] rigIds = null);
    }
}