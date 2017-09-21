using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts
{
    public interface IMarketInfoMonitorStorage
    {
        Coin[] GetCoins();
        Exchange[] GetExchanges();
        void StoreExchangeCoins(ExchangeCoin[] coins);
        void StoreMarketPrices(ExchangeMarketPrice[] prices);
    }
}
