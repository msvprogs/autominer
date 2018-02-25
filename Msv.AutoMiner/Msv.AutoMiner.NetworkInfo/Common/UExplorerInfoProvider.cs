using System;
using System.Linq;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    public class UExplorerInfoProvider : NetworkInfoProviderBase
    {
        private const string LastBlockRequestUrl =
            "datatables/blocks?draw=1&columns%5B0%5D%5Bdata%5D=Height&columns%5B0%5D%5Bname%5D=Height&columns%5B0%5D%5Bsearchable%5D=true"
            + "&columns%5B0%5D%5Borderable%5D=false&columns%5B0%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B0%5D%5Bsearch%5D%5Bregex%5D=false"
            + "&columns%5B1%5D%5Bdata%5D=Time&columns%5B1%5D%5Bname%5D=Time&columns%5B1%5D%5Bsearchable%5D=true&columns%5B1%5D%5B"
            + "orderable%5D=false&columns%5B1%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B1%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B2%5D%5B"
            + "data%5D=TotalTx&columns%5B2%5D%5Bname%5D=TotalTx&columns%5B2%5D%5Bsearchable%5D=true&columns%5B2%5D%5Borderable%5D=false"
            + "&columns%5B2%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B2%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B3%5D%5Bdata%5D=Type&columns"
            + "%5B3%5D%5Bname%5D=Type&columns%5B3%5D%5Bsearchable%5D=true&columns%5B3%5D%5Borderable%5D=false&columns%5B3%5D%5Bsearch%5D%5B"
            + "value%5D=&columns%5B3%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B4%5D%5Bdata%5D=Reward&columns%5B4%5D%5Bname%5D=Reward&"
            + "columns%5B4%5D%5Bsearchable%5D=true&columns%5B4%5D%5Borderable%5D=false&columns%5B4%5D%5Bsearch%5D%5Bvalue%5D=&columns"
            + "%5B4%5D%5Bsearch%5D%5Bregex%5D=false&start=0&length=15&search%5Bvalue%5D=&search%5Bregex%5D=false";

        private readonly IWebClient m_WebClient;
        private readonly Uri m_BaseUrl;

        public UExplorerInfoProvider(IWebClient webClient, string baseUrl)
        {
            if (baseUrl == null) 
                throw new ArgumentNullException(nameof(baseUrl));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_BaseUrl = new Uri(baseUrl);
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic stats = JsonConvert.DeserializeObject<JArray>(m_WebClient.DownloadString(
                new Uri(m_BaseUrl, "/api/chart/stat")))[0];
            var lastBlock = ((JArray)JsonConvert.DeserializeObject<dynamic>(m_WebClient.DownloadString(
                    new Uri(m_BaseUrl, LastBlockRequestUrl))).data)
                .Cast<dynamic>()
                .First(x => (string)x.Type == "POW");

            return new CoinNetworkStatistics
            {
                Difficulty = (double) stats.Difficulty,
                Height = (long) stats.Block,
                LastBlockTime = DateTimeHelper.FromIso8601((string) lastBlock.TimeNormal),
                MasternodeCount = (int) stats.Masternode,
                TotalSupply = (double) stats.Supply,
                NetHashRate = (double) stats.Network * 1e6
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(m_BaseUrl, $"transactions/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(m_BaseUrl, $"wallets/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(m_BaseUrl, $"blocks/{blockHash}");
    }
}
