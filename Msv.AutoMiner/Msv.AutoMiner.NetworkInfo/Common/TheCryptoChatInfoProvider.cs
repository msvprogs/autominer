using System;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    public class TheCryptoChatInfoProvider : NetworkInfoProviderBase
    {
        private readonly Uri m_BaseUri;
        private readonly IWebClient m_WebClient;

        public TheCryptoChatInfoProvider(IWebClient webClient, string coinName)
        {
            if (string.IsNullOrEmpty(coinName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(coinName));

            m_BaseUri = new Uri($"http://{coinName}.thecryptochat.net/");
            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var miningInfo = GetApiResponse("getmininginfo");
            return new CoinNetworkStatistics
            {
                BlockReward = GetBlockReward(miningInfo),
                Difficulty = miningInfo.data.difficulty is JValue
                    ? (double)miningInfo.data.difficulty
                    : ((double?)miningInfo.data.difficulty["proof-of-work"] ?? 0),
                NetHashRate = (long) (miningInfo.data.netmhashps != null
                    ? (double) miningInfo.data.netmhashps * 1e6
                    : miningInfo.data.networkhashps != null
                        ? (double) miningInfo.data.networkhashps
                        : 0),
                Height = (long) miningInfo.data.blocks
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(m_BaseUri, $"/tx.php?tx={hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(m_BaseUri, $"/address.php?address={address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(m_BaseUri, $"/block.php?hash={blockHash}");

        private static double? GetBlockReward(dynamic miningInfo)
        {
            if (miningInfo.data?.blockvalue != null)
                return (double)miningInfo.data.blockvalue / 1e8;
            return null;
        }

        private dynamic GetApiResponse(string requestType)
            => JsonConvert.DeserializeObject(m_WebClient.DownloadString(
                new Uri(m_BaseUri, $"/api_fetch.php?method={requestType}")));
    }
}
