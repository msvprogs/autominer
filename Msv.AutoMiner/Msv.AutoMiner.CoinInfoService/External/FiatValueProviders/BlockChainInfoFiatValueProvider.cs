using System;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.FiatValueProviders
{
    //API: https://blockchain.info/api/blockchain_api
    public class BlockChainInfoFiatValueProvider : IFiatValueProvider
    {
        private readonly IWebClient m_WebClient;

        public BlockChainInfoFiatValueProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CurrencyFiatValue[] GetFiatValues()
        {
            dynamic stats = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString("https://api.blockchain.info/stats"));
            return new[]
            {
                new CurrencyFiatValue
                {
                    CurrencySymbol = "BTC",
                    FiatCurrencySymbol = "USD",
                    Value = (double) stats.market_price_usd
                }
            };
        }
    }
}
