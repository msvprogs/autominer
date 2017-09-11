using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Storage.Contracts
{
    public interface IAutomaticMinerChangerStorage
    {
        void SaveProfitabilities(CoinProfitability[] profitabilities);
        void SaveChangeEvents(MiningChangeEvent[] miningChangeEvent);
    }
}