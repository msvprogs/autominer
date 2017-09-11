using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Msv.AutoMiner.Commons;

namespace Msv.AutoMiner.Service.External.Exchanges
{
    //API: http://coinexchangeio.github.io/slate
    public class CoinExchangeMarketInfoProvider : WebDownloaderBase, ICoinMarketInfoProvider
    {
        public CoinMarketInfo[] GetCoinMarketInfos(string[] currencySymbols)
        {
            var markets = DoRequest("getmarkets");
            var marketCodeMapping = markets
                .Cast<dynamic>()
                .Where(x => (string) x.BaseCurrencyCode == "BTC")
                .Where(x => (bool) x.Active)
                .Select(x => new
                {
                    Id = (int) x.MarketID,
                    Symbol = (string) x.MarketAssetCode
                })
                .ToDictionary(x => x.Id, x => x.Symbol);

            var summaries = DoRequest("getmarketsummaries");
            return summaries
                .Cast<dynamic>()
                .Select(x => new
                {
                    Data = x,
                    Symbol = marketCodeMapping.TryGetValue((int) x.MarketID)
                })
                .Where(x => x.Symbol != null)
                .Select(x => new CoinMarketInfo
                {
                    Symbol = x.Symbol,
                    BtcHighestBid = (double) x.Data.BidPrice,
                    BtcLowestAsk = (double)x.Data.AskPrice
                })
                .ToArray();
        }

        private JArray DoRequest(string command)
        {
            dynamic response = JsonConvert.DeserializeObject(
                DownloadString($"https://www.coinexchange.io/api/v1/{command}",
                new Dictionary<HttpRequestHeader, string>
                {
                    [HttpRequestHeader.UserAgent] = UserAgent
                }));
            if ((int)response.success != 1)
                throw new ApplicationException((string)response.message);
            return response.result;
        }
    }
}
