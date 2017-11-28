using System.Threading.Tasks;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Storage
{
    public interface ICoinInfoControllerStorage
    {
        Task<CoinAlgorithm[]> GetAlgorithms();
        Task<CoinNetworkInfo[]> GetNetworkInfos(ValueAggregationType aggregationType);
        ExchangeMarketPrice[] GetExchangeMarketPrices(ValueAggregationType aggregationType);
        Task<Coin> GetBtcCurrency();
    }
}
