using System;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common
{
    //MinerGate API: https://github.com/MinerGate/minergate-api
    public class MinerGateInfoProvider : INetworkInfoProvider
    {
        private readonly IWebClient m_WebClient;
        private readonly string m_CurrencySymbol;

        public MinerGateInfoProvider(IWebClient webClient, string currencySymbol)
        {
            if (string.IsNullOrEmpty(currencySymbol))
                throw new ArgumentException("Value cannot be null or empty.", nameof(currencySymbol));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_CurrencySymbol = currencySymbol.ToLowerInvariant();
        }

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic json = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString($"https://api.minergate.com/1.0/{m_CurrencySymbol}/status"));
            return new CoinNetworkStatistics
            {
                BlockReward = (double) json.reward,
                Difficulty = (double) json.difficulty,
                NetHashRate = (long) json.instantHashrate,
                Height = (long) json.height
            };
        }
    }
}