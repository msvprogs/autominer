using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Logic.Profitability
{
    public interface IProfitabilityCalculator
    {
        double CalculateCoinsPerDay(Coin coin, CoinNetworkInfo networkInfo, long yourHashRate);
    }
}
