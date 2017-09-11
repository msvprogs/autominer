using System;
using System.Linq;
using Msv.AutoMiner.Commons;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Exchanges
{
    //API: https://tradesatoshi.com/Home/Api
    public class TradeSatoshiMarketInfoProvider : WebDownloaderBase, ICoinMarketInfoProvider
    {
        public CoinMarketInfo[] GetCoinMarketInfos(string[] currencySymbols)
        {
            var markets = DoRequest<JArray>("getmarketsummaries");
            var currencies = DoRequest<JArray>("getcurrencies");
            var txFees = currencies.Cast<dynamic>()
                .Where(x => (string) x.status == "OK")
                .ToDictionary(x => (string) x.currency, x => (double) x.txFee);

            return markets.Cast<dynamic>()
                .Where(x => ((string) x.market).EndsWith("_BTC"))
                .Select(x => new CoinMarketInfo
                {
                    Symbol = ((string) x.market).Split('_')[0],
                    BtcHighestBid = (double) x.bid,
                    BtcLowestAsk = (double) x.ask
                })
                .Do(x => x.WithdrawalFee = txFees.TryGetValue(x.Symbol))
                .Where(x => currencySymbols.Contains(x.Symbol))
                .ToArray();
        }

        private T DoRequest<T>(string command)
            where T : JToken
        {
            var json = JsonConvert.DeserializeObject<JObject>(
                DownloadString($"https://tradesatoshi.com/api/public/{command}"));
            if (!json["success"].Value<bool>())
                throw new ApplicationException(json["message"].Value<string>());
            return (T)json["result"];
        }
    }
}
