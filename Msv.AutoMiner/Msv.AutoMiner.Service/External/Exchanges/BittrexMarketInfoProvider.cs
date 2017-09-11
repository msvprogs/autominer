using System;
using System.Linq;
using Msv.AutoMiner.Commons;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Exchanges
{
    //API: https://bittrex.com/Home/Api
    public class BittrexMarketInfoProvider : WebDownloaderBase, ICoinMarketInfoProvider
    {
        private const double ConversionFeePercent = 0.25;

        public CoinMarketInfo[] GetCoinMarketInfos(string[] currencySymbols)
        {
            var markets = DoRequest<JArray>("getmarketsummaries");
            var currencies = DoRequest<JArray>("getcurrencies");
            var txFees = currencies.Cast<dynamic>()
                .Where(x => (bool) x.IsActive)
                .ToDictionary(x => (string)x.Currency, x => (double)x.TxFee);

            return markets.Cast<dynamic>()
                .Where(x => ((string)x.MarketName).StartsWith("BTC-"))
                .Where(x => (double)x.Volume > 0 && (int)x.OpenBuyOrders > 0)
                .Select(x => new CoinMarketInfo
                {
                    Symbol = ((string)x.MarketName).Split('-')[1],
                    BtcHighestBid = (double)x.Bid,
                    BtcLowestAsk = (double)x.Ask,
                })
                .Do(x => x.WithdrawalFee = txFees.TryGetValue(x.Symbol))
                .Do(x => x.ConversionFeePercent = ConversionFeePercent)
                .ToArray();
        }

        private T DoRequest<T>(string command)
            where T : JToken
        {
            var json = JsonConvert.DeserializeObject<JObject>(
                DownloadString($"https://bittrex.com/api/v1.1/public/{command}"));
            if (!json["success"].Value<bool>())
                throw new ApplicationException(json["message"].Value<string>());
            return (T)json["result"];
        }
    }
}
