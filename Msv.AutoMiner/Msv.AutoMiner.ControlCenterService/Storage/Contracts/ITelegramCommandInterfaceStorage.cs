using System.Collections.Generic;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Storage.Contracts
{
    public interface ITelegramCommandInterfaceStorage
    {
        void StoreTelegramUser(TelegramUser user);
        KeyValuePair<string, Heartbeat>[] GetLastHeartbeats(string[] rigNames);
        Dictionary<int, PoolAccountState> GetLastPoolAccountStates(int[] poolIds);
        Coin[] GetCoins();
    }
}
