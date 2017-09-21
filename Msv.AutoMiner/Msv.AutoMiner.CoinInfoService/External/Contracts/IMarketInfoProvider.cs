using Msv.AutoMiner.CoinInfoService.External.Data;

namespace Msv.AutoMiner.CoinInfoService.External.Contracts
{
    public interface IMarketInfoProvider
    {
        bool HasMarketsCountLimit { get; }
        ExchangeCurrencyInfo[] GetCurrencies();
        CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos);
    }
}
