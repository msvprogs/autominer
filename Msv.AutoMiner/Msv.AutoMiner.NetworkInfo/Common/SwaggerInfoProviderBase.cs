using System;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    public abstract class SwaggerInfoProviderBase : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;
        private readonly Uri m_BaseUrl;

        protected SwaggerInfoProviderBase(IWebClient webClient, string baseUrl)
        {
            if (baseUrl == null)
                throw new ArgumentNullException(nameof(baseUrl));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_BaseUrl = new Uri(baseUrl);
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic stats = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(m_BaseUrl, "/Blockchain/GetMiningInfo")));
            var height = (long) stats.result.blocks;
            dynamic lastBlockHash = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(m_BaseUrl, "/Blockchain/GetBlockHash/" + height)));
            dynamic lastBlockInfo = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(m_BaseUrl, "/Blockchain/GetBlock/" + (string)lastBlockHash.result)));

            return new CoinNetworkStatistics
            {
                Height = height,
                BlockReward = GetBlockReward(height),
                Difficulty = (double) stats.result.difficulty,
                NetHashRate = (long) stats.result.networkhashps,
                LastBlockTime = DateTimeHelper.ToDateTimeUtc((long)lastBlockInfo.time)
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(m_BaseUrl, $"/Blockchain/GetRawTransaction/{hash}");

        public override Uri CreateAddressUrl(string address)
            => null;

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(m_BaseUrl, $"/Blockchain/GetBlock/{blockHash}");

        protected abstract double GetBlockReward(long height);
    }
}
