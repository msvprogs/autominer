using System;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Service.External.Pools
{
    public class TbfPoolInfoProvider : WebDownloaderBase, IPoolInfoProvider
    {
        private readonly string m_BaseUrl;
        private readonly string m_ApiKey;

        public TbfPoolInfoProvider(string baseUrl, string apiKey)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("Value cannot be null or empty.", nameof(apiKey));

            m_BaseUrl = baseUrl;
            m_ApiKey = apiKey;
        }

        public PoolInfo GetInfo(DateTime minPaymentDate)
        {
            var url = new UriBuilder(m_BaseUrl)
            {
                Query = "api_key=" + Uri.EscapeDataString(m_ApiKey)
            }.Uri.ToString();
            dynamic accountJson = JsonConvert.DeserializeObject(DownloadString(url));
            var accountInfo = new PoolAccountInfo
            {
                ConfirmedBalance = (double)accountJson.confirmed_rewards,
                UnconfirmedBalance = (double)accountJson.round_estimate,
                Hashrate = (long)((double)accountJson.total_hashrate * 1000),
                ValidShares = (int)accountJson.round_shares,
                InvalidShares = 0
            };
            dynamic stateJson = JsonConvert.DeserializeObject(DownloadString(m_BaseUrl));
            var state = new PoolState
            {
                TotalHashRate = (long)((double)stateJson.hashrate * 1000),
                TotalWorkers = (int)stateJson.workers
            };
            return new PoolInfo(accountInfo, state, new PoolPaymentData[0]);
        }
    }
}
