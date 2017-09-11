using System;
using System.Linq;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Exchanges
{
    public class CoinsMarketsMarketInfoProvider : WebDownloaderBase, ICoinMarketInfoProvider
    {
        public CoinMarketInfo[] GetCoinMarketInfos(string[] currencySymbols)
            => JsonConvert.DeserializeObject<JObject>(
                    DownloadString("https://coinsmarkets.com/apicoin.php"))
                .Properties()
                .Where(x => x.Name.StartsWith("BTC_", StringComparison.InvariantCultureIgnoreCase))
                .Select(x => new
                {
                    Symbol = x.Name.Split('_')[1],
                    Data = (dynamic) x.Value
                })
                .Select(x => new CoinMarketInfo
                {
                    Symbol = x.Symbol.ToUpperInvariant(),
                    BtcLowestAsk = x.Data.lowestAsk,
                    BtcHighestBid = x.Data.highestBid
                })
                .Where(x => currencySymbols.Contains(x.Symbol))
                .ToArray();
    }
}
