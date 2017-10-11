using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.FiatValueProviders
{
    //API: https://developers.coinbase.com/api/v2
    public class CoinbaseFiatValueProvider : IFiatValueProvider
    {
        private readonly IWebClient m_WebClient;

        public CoinbaseFiatValueProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CurrencyFiatValue[] GetFiatValues()
        {
            dynamic response = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString("https://api.coinbase.com/v2/exchange-rates?currency=BTC"));
            return ((JObject)response.data.rates)
                .Properties()
                .Where(x => x.Name == "USD")
                .Select(x => new CurrencyFiatValue
                {
                    CurrencySymbol = "BTC",
                    FiatCurrencySymbol = x.Name,
                    Value = ParsingHelper.ParseDouble(x.Value.Value<string>())
                })
                .ToArray();
        }
    }
}
