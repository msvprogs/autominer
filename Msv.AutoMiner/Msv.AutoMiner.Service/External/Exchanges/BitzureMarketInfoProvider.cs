using System;
using System.Linq;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Exchanges
{
    //API: https://bitzure.com/documents/api_v2
    public class BitzureMarketInfoProvider : WebDownloaderBase, ICoinMarketInfoProvider
    {
        public CoinMarketInfo[] GetCoinMarketInfos(string[] currencySymbols)
            => JsonConvert.DeserializeObject<JObject>(
                    DownloadString("https://bitzure.com/api/v2/tickers.json"))
                .Properties()
                .Cast<dynamic>()
                .Select(x => new
                {
                    Market = (string) x.Name,
                    Buy = (double) x.Value.ticker.buy,
                    Sell = (double) x.Value.ticker.sell
                })
                .Where(x => x.Market != null && x.Market.EndsWith("btc", StringComparison.InvariantCultureIgnoreCase))
                .Select(x => new CoinMarketInfo
                {
                    Symbol = x.Market.Substring(0, x.Market.Length - 3).ToUpperInvariant(),
                    BtcHighestBid = x.Buy,
                    BtcLowestAsk = x.Sell
                })
                .ToArray();
    }
}
