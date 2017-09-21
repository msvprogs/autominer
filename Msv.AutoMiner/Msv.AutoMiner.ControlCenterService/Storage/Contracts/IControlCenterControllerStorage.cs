using System;
using System.Threading.Tasks;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Storage.Contracts
{
    public interface IControlCenterControllerStorage
    {
        Task<Rig> GetRigByName(string name);
        Task SaveRig(Rig rig);
        Task SaveHeartbeat(RigHeartbeat heartbeat);
        Task<RigCommand> GetNextCommand(int rigId);
        Task MarkCommandAsSent(int commandId);
        Task<Pool[]> GetActivePools(Guid[] coinIds);
    }
}
