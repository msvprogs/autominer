using System;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.External.Contracts;

namespace Msv.AutoMiner.CoinInfoService.External
{
    public class MarketInfoProviderFactory : IMarketInfoProviderFactory
    {
        private readonly IWebClient m_WebClient;

        public MarketInfoProviderFactory(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public IMarketInfoProvider Create(ExchangeType exchange)
        {
            switch (exchange)
            {
                case ExchangeType.Bittrex:
                    return new BittrexMarketInfoProvider(m_WebClient);
                case ExchangeType.Cryptopia:
                    return new CryptopiaMarketInfoProvider(m_WebClient);
                case ExchangeType.Poloniex:
                    return new PoloniexMarketInfoProvider(m_WebClient);
                case ExchangeType.CoinExchange:
                    return new CoinExchangeMarketInfoProvider(m_WebClient);
                case ExchangeType.YoBit:
                    return new YoBitMarketInfoProvider(m_WebClient);
                case ExchangeType.TradeSatoshi:
                    return new TradeSatoshiMarketInfoProvider(m_WebClient);
                case ExchangeType.CoinsMarkets:
                    return new CoinsMarketsMarketInfoProvider(m_WebClient);
                case ExchangeType.Novaexchange:
                    return new NovaexchangeMarketInfoProvider(m_WebClient);
                case ExchangeType.LiveCoin:
                    return new LiveCoinMarketInfoProvider(m_WebClient);
                case ExchangeType.StocksExchange:
                    return new StocksExchangeMarketInfoProvider(m_WebClient);
                case ExchangeType.BtcAlpha:
                    return new BtcAlphaMarketInfoProvider(m_WebClient);
                case ExchangeType.CryptoBridge:
                    return new CryptoBridgeMarketInfoProvider(m_WebClient);
                default:
                    return new DummyMarketInfoProvider();
            }
        }

        private class DummyMarketInfoProvider : IMarketInfoProvider
        {
            public bool HasMarketsCountLimit => false;

            public ExchangeCurrencyInfo[] GetCurrencies() 
                => new ExchangeCurrencyInfo[0];

            public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos)
                => new CurrencyMarketInfo[0];
        }
    }
}
