using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.Infrastructure.Data;

namespace Msv.AutoMiner.Service.Infrastructure.Contracts
{
    public interface IAutomaticMinerChanger
    {
        CoinProfitabilityData[] CurrentProfitabilityTable { get; }

        MiningMode CurrentMode { get; }
        CoinBaseInfo[] CurrentCoins { get; }
    }
}
