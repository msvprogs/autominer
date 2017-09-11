using System.Linq;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Exchanges
{
    //API: https://yobit.net/en/api/
    public class YoBitMarketInfoProvider : WebDownloaderBase, ICoinMarketInfoProvider
    {
        public CoinMarketInfo[] GetCoinMarketInfos(string[] currencySymbols)
        {
            var pairs = string.Join("-", currencySymbols.Select(x => x.ToLowerInvariant() + "_btc"));
            var markets = JsonConvert.DeserializeObject<JObject>(
                DownloadString($"https://yobit.net/api/3/ticker/{pairs}"));
            return markets.Properties()
                .Cast<dynamic>()
                .Select(x => new CoinMarketInfo
                {
                    Symbol = ((string)x.Name).Split('_')[0].ToUpperInvariant(),
                    BtcHighestBid = (double)x.Value.buy,
                    BtcLowestAsk = (double)x.Value.sell
                })
                .ToArray();
        }
    }
}
