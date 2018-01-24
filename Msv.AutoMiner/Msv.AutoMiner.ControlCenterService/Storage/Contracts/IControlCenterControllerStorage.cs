using System;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Storage.Contracts
{
    public interface IControlCenterControllerStorage
    {
        Rig GetRigByName(string name);
        Rig GetRigById(int rigId);
        void SaveRig(Rig rig);
        void SaveHeartbeat(RigHeartbeat heartbeat);
        void SaveMiningStates(RigMiningState[] miningStates);
        RigCommand GetNextCommand(int rigId);
        void MarkCommandAsSent(int commandId);
        Pool[] GetActivePools(Guid[] coinIds);
        void SaveProfitabilities(CoinProfitability[] profitabilities);
        Wallet GetBitCoinMiningTarget();
    }
}
