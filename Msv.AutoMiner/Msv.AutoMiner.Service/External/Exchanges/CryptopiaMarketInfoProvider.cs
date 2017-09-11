using System;
using System.Linq;
using Msv.AutoMiner.Commons;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Exchanges
{
    //Public API: https://www.cryptopia.co.nz/Forum/Thread/255
    //Private API: https://www.cryptopia.co.nz/Forum/Thread/256
    public class CryptopiaMarketInfoProvider : WebDownloaderBase, ICoinMarketInfoProvider
    {
        public CoinMarketInfo[] GetCoinMarketInfos(string[] currencySymbols)
        {
            var markets = DoRequest<JArray>("GetMarkets");
            var currencies = DoRequest<JArray>("GetCurrencies");
            var tradePairs = DoRequest<JArray>("GetTradePairs");
            var txFeeData = currencies.Cast<dynamic>()
                .Where(x => (string)x.Status == "OK")
                .ToDictionary(x => (string) x.Symbol, x => (double) x.WithdrawFee);
            var tradeFeeData = tradePairs.Cast<dynamic>()
                .Where(x => (string) x.BaseSymbol == "BTC")
                .ToDictionary(x => (string) x.Symbol, x => (double) x.TradeFee);
            return markets.Cast<dynamic>()
                .Where(x => ((string)x.Label).EndsWith("/BTC")
                            || ((string)x.Label).EndsWith("/LTC"))
                .Where(x => (double)x.Volume > 0 && (double)x.BuyVolume > 0)
                .Select(x => new
                {
                    Symbols = ((string)x.Label).Split('/'),
                    HighestBid = (double)x.BidPrice,
                    LowestAsk = (double)x.AskPrice
                })
                .GroupBy(x => x.Symbols[0])
                .Select(x => new CoinMarketInfo
                {
                    Symbol = x.Key,
                    BtcHighestBid = (x.FirstOrDefault(y => y.Symbols[1] == "BTC")?.HighestBid).GetValueOrDefault(),
                    BtcLowestAsk = (x.FirstOrDefault(y => y.Symbols[1] == "BTC")?.LowestAsk).GetValueOrDefault(),
                    LtcHighestBid = (x.FirstOrDefault(y => y.Symbols[1] == "LTC")?.HighestBid).GetValueOrDefault(),
                    LtcLowestAsk = (x.FirstOrDefault(y => y.Symbols[1] == "LTC")?.LowestAsk).GetValueOrDefault(),
                    WithdrawalFee = txFeeData.TryGetValue(x.Key),
                    ConversionFeePercent = tradeFeeData.TryGetValue(x.Key)
                })
                .ToArray();
        }

        private T DoRequest<T>(string command)
            where T : JToken
        {
            var json = JsonConvert.DeserializeObject<JObject>(
                DownloadString($"https://www.cryptopia.co.nz/api/{command}"));
            if (!json["Success"].Value<bool>())
                throw new ApplicationException(json["Message"].Value<string>() + " " + json["Error"].Value<string>());
            return (T)json["Data"];
        }
    }
}
