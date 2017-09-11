using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Msv.AutoMiner.Service.External.Exchanges;

namespace Msv.AutoMiner.Service.External
{
    public class CoinMarketInfoProviderFactory : ICoinMarketInfoProviderFactory
    {
        public ICoinMarketInfoProvider Create(ExchangeType exchange)
        {
            switch (exchange)
            {
                case ExchangeType.Bittrex:
                    return new BittrexMarketInfoProvider();
                case ExchangeType.Cryptopia:
                    return new CryptopiaMarketInfoProvider();
                case ExchangeType.Poloniex:
                    return new PoloniexMarketInfoProvider();
                case ExchangeType.CoinExchange:
                    return new CoinExchangeMarketInfoProvider();
                case ExchangeType.YoBit:
                    return new YoBitMarketInfoProvider();
                case ExchangeType.TradeSatoshi:
                    return new TradeSatoshiMarketInfoProvider();
                case ExchangeType.CoinsMarkets:
                    return new CoinsMarketsMarketInfoProvider();
                case ExchangeType.Novaexchange:
                    return new NovaexchangeMarketInfoProvider();
                case ExchangeType.Bitzure:
                    return new BitzureMarketInfoProvider();
                default:
                    return new DummyMarketInfoProvider();
            }
        }

        private class DummyMarketInfoProvider : ICoinMarketInfoProvider
        {
            public CoinMarketInfo[] GetCoinMarketInfos(string[] symbols) => new CoinMarketInfo[0];
        }
    }
}
