using System.Collections.Generic;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Storage.Contracts
{
    public interface ITelegramCommandInterfaceStorage
    {
        void StoreTelegramUser(TelegramUser user);
        int[] GetRigIds(string[] names);
        Dictionary<int, string> GetRigNames(int[] ids);
        Coin[] GetCoins();
    }
}
