using System;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.External.FiatValueProviders;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.External
{
    public class FiatValueProviderFactory : IFiatValueProviderFactory
    {
        private readonly IWebClient m_WebClient;

        public FiatValueProviderFactory(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public IFiatValueProvider Create(CoinFiatValueSource source)
        {
            switch (source)
            {
                case CoinFiatValueSource.BlockChainInfo:
                    return new BlockChainInfoFiatValueProvider(m_WebClient);
                case CoinFiatValueSource.Bitfinex:
                    return new BitfinexFiatValueProvider(m_WebClient);
                case CoinFiatValueSource.Coinbase:
                    return new CoinbaseFiatValueProvider(m_WebClient);
                default:
                    return new DummyFiatValueProvider();
            }
        }

        private class DummyFiatValueProvider : IFiatValueProvider
        {
            public CurrencyFiatValue[] GetFiatValues() => new CurrencyFiatValue[0];
        }
    }
}
