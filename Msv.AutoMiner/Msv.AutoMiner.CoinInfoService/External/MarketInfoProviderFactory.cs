using System;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders;
using Msv.AutoMiner.CoinInfoService.Storage;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Exchanges.Api;

namespace Msv.AutoMiner.CoinInfoService.External
{
    public class MarketInfoProviderFactory : IMarketInfoProviderFactory
    {
        private readonly IWebClient m_WebClient;
        private readonly IMarketInfoProviderFactoryStorage m_Storage;

        public MarketInfoProviderFactory(IWebClient webClient, IMarketInfoProviderFactoryStorage storage)
        {
            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public IMarketInfoProvider Create(ExchangeType exchange)
        {
            switch (exchange)
            {
                case ExchangeType.Bittrex:
                    return new BittrexMarketInfoProvider(new BittrexExchangeApi(m_WebClient));
                case ExchangeType.Cryptopia:
                    return new CryptopiaMarketInfoProvider(new CryptopiaExchangeApi(m_WebClient));
                case ExchangeType.Poloniex:
                    return new PoloniexMarketInfoProvider(new PoloniexExchangeApi(m_WebClient));
                case ExchangeType.CoinExchange:
                    return new CoinExchangeMarketInfoProvider(new CoinExchangeExchangeApi(m_WebClient));
                case ExchangeType.YoBit:
                    return new YoBitMarketInfoProvider(new YoBitExchangeApi(m_WebClient));
                case ExchangeType.TradeSatoshi:
                    return new TradeSatoshiMarketInfoProvider(new TradeSatoshiExchangeApi(m_WebClient));
                case ExchangeType.CoinsMarkets:
                    return new CoinsMarketsMarketInfoProvider(m_WebClient);
                case ExchangeType.Novaexchange:
                    var novaExchange = m_Storage.GetExchange(ExchangeType.Novaexchange);
                    return new NovaexchangeMarketInfoProvider(
                        new NovaexchangeExchangeApi(m_WebClient), novaExchange?.PublicKey, novaExchange?.PrivateKey);
                case ExchangeType.LiveCoin:
                    return new LiveCoinMarketInfoProvider(new LiveCoinExchangeApi(m_WebClient));
                case ExchangeType.StocksExchange:
                    return new StocksExchangeMarketInfoProvider(new StocksExchangeExchangeApi(m_WebClient));
                case ExchangeType.BtcAlpha:
                    return new BtcAlphaMarketInfoProvider(new BtcAlphaExchangeApi(m_WebClient));
                case ExchangeType.CryptoBridge:
                    return new CryptoBridgeMarketInfoProvider(new CryptoBridgeExchangeApi(m_WebClient));
                case ExchangeType.SouthXchange:
                    return new SouthXchangeMarketInfoProvider(new SouthXchangeExchangeApi(m_WebClient));
                case ExchangeType.Graviex:
                    return new GraviexMarketInfoProvider(new GraviexExchangeApi(m_WebClient));
                default:
                    return new DummyMarketInfoProvider();
            }
        }

        private class DummyMarketInfoProvider : IMarketInfoProvider
        {
            public bool HasMarketsCountLimit => false;
            public TimeSpan? RequestInterval => null;

            public ExchangeCurrencyInfo[] GetCurrencies() 
                => new ExchangeCurrencyInfo[0];

            public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos)
                => new CurrencyMarketInfo[0];
        }
    }
}
