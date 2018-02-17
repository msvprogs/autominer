using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts
{
    public interface IMarketInfoMonitorStorage
    {
        Coin[] GetCoins();
        Exchange[] GetExchanges();
        void StoreExchangeCurrencies(ExchangeCurrency[] currencies);
        void StoreMarketPrices(ExchangeMarketPrice[] prices);
    }
}
