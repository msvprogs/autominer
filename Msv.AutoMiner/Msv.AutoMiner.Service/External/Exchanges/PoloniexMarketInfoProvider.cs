using System.Linq;
using Msv.AutoMiner.Commons;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Exchanges
{
    //API: https://poloniex.com/support/api/
    public class PoloniexMarketInfoProvider : WebDownloaderBase, ICoinMarketInfoProvider
    {
        private const double ConversionFeePercent = 0.25;

        public CoinMarketInfo[] GetCoinMarketInfos(string[] currencySymbols)
        {
            var currencies = DoRequest<JObject>("returnCurrencies");
            var txFees = currencies.Properties()
                .Cast<dynamic>()
                .Where(x => (int)x.Value.disabled == 0)
                .ToDictionary(x => (string)x.Name, x => (double)x.Value.txFee);

            return DoRequest<JObject>("returnTicker")
                .Properties()
                .Where(x => x.Name.StartsWith("BTC_"))
                .Select(x => new
                {
                    x.Name,
                    Value = (dynamic)x.Value
                })
                .Where(x => (byte)x.Value.isFrozen == 0 && (decimal)x.Value.quoteVolume > 0)
                .Select(x => new CoinMarketInfo
                {
                    Symbol = x.Name.Split('_')[1],
                    BtcHighestBid = (double)x.Value.highestBid,
                    BtcLowestAsk = (double)x.Value.lowestAsk
                })
                .Do(x => x.WithdrawalFee = txFees.TryGetValue(x.Symbol))
                .Do(x => x.ConversionFeePercent = ConversionFeePercent)
                .ToArray();
        }

        private T DoRequest<T>(string command)
            where T : JToken => 
            JsonConvert.DeserializeObject<T>(DownloadString($"https://poloniex.com/public?command={command}"));
    }
}
