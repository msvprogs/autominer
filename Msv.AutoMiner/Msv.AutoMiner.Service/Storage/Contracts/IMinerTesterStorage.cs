using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Storage.Contracts
{
    public interface IMinerTesterStorage
    {
        Coin[] GetCoins();
        void UpdateAlgorithmHashRate(CoinAlgorithm algorithm, long hashRate, double powerUsage);
    }
}
