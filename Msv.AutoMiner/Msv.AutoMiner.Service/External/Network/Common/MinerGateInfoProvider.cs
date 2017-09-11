using System;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Service.External.Network.Common
{
    //MinerGate API: https://github.com/MinerGate/minergate-api
    public class MinerGateInfoProvider : WebDownloaderBase, ICoinNetworkInfoProvider
    {
        private readonly string m_CurrencySymbol;

        public MinerGateInfoProvider(string currencySymbol)
        {
            if (string.IsNullOrEmpty(currencySymbol))
                throw new ArgumentException("Value cannot be null or empty.", nameof(currencySymbol));

            m_CurrencySymbol = currencySymbol.ToLowerInvariant();
        }

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic json = JsonConvert.DeserializeObject(
                DownloadString($"https://api.minergate.com/1.0/{m_CurrencySymbol}/status"));
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
