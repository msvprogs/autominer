using System;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.FiatValueProviders
{
    //API: https://docs.bitfinex.com/v1/reference
    public class BitfinexFiatValueProvider : IFiatValueProvider
    {
        private readonly IWebClient m_WebClient;

        public BitfinexFiatValueProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CurrencyFiatValue[] GetFiatValues()
        {
            dynamic response = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString("https://api.bitfinex.com/v1/pubticker/btcusd"));
            return new[]
            {
                new CurrencyFiatValue
                {
                    CurrencySymbol = "BTC",
                    FiatCurrencySymbol = "USD",
                    Value = ParsingHelper.ParseDouble((string)response.last_price)
                }
            };
        }
    }
}
