using System;
using System.Linq;
using System.Net;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Exchanges
{
    //API: https://novaexchange.com/remote/faq/
    public class NovaexchangeMarketInfoProvider : WebDownloaderBase, ICoinMarketInfoProvider
    {
        public CoinMarketInfo[] GetCoinMarketInfos(string[] currencySymbols)
            => DoRequest<JArray>("markets", "markets")
                .Cast<dynamic>()
                .Where(x => (string)x.basecurrency == "BTC")
                .Select(x => new CoinMarketInfo
                {
                    Symbol = (string) x.currency,
                    BtcHighestBid = (double) x.bid,
                    BtcLowestAsk = (double) x.ask
                })
                .ToArray();

        private T DoRequest<T>(string command, string resultFieldKey)
            where T : JToken
        {
            string jsonStr;
            try
            {
                jsonStr = DownloadString($"https://novaexchange.com/remote/v2/{command}", timeout: TimeSpan.FromSeconds(20));
            }
            catch (WebException)
            {
                //To avoid IP range blocking
                jsonStr = DownloadString($"http://0s.nzxxmylfpbrwqylom5ss4y3pnu.cmle.ru/remote/v2/{command}");
            }

            var json = JsonConvert.DeserializeObject<JObject>(jsonStr);
            if (json["status"].Value<string>() != "success")
                throw new ApplicationException(json["message"].Value<string>());
            return (T) json[resultFieldKey];
        }
    }
}
