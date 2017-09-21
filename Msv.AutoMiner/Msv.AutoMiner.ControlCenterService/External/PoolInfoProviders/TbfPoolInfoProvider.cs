using System;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.ControlCenterService.External.PoolInfoProviders
{
    public class TbfPoolInfoProvider : IPoolInfoProvider
    {
        private readonly IWebClient m_WebClient;
        private readonly string m_BaseUrl;
        private readonly string m_ApiKey;

        public TbfPoolInfoProvider(IWebClient webClient, string baseUrl, string apiKey)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("Value cannot be null or empty.", nameof(apiKey));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_BaseUrl = baseUrl;
            m_ApiKey = apiKey;
        }

        public PoolInfo GetInfo(DateTime minPaymentDate)
        {
            var url = new UriBuilder(m_BaseUrl)
            {
                Query = "api_key=" + Uri.EscapeDataString(m_ApiKey)
            }.Uri.ToString();
            dynamic accountJson = JsonConvert.DeserializeObject(m_WebClient.DownloadString(url));
            var accountInfo = new PoolAccountInfo
            {
                ConfirmedBalance = (double)accountJson.confirmed_rewards,
                UnconfirmedBalance = (double)accountJson.round_estimate,
                HashRate = (long)((double)accountJson.total_hashrate * 1000),
                ValidShares = (int)accountJson.round_shares,
                InvalidShares = 0
            };
            dynamic stateJson = JsonConvert.DeserializeObject(m_WebClient.DownloadString(m_BaseUrl));
            var state = new PoolState
            {
                TotalHashRate = (long)((double)stateJson.hashrate * 1000),
                TotalWorkers = (int)stateJson.workers
            };
            return new PoolInfo
            {
                AccountInfo = accountInfo,
                State = state,
                PaymentsData = new PoolPaymentData[0]
            };
        }
    }
}
