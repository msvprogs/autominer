﻿using System;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common
{
    public class OpenEthereumPoolInfoProvider : INetworkInfoProvider
    {
        private readonly IWebClient m_WebClient;
        private readonly string m_StatsUrl;

        public OpenEthereumPoolInfoProvider(IWebClient webClient, string statsUrl)
        {
            if (string.IsNullOrEmpty(statsUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(statsUrl));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_StatsUrl = statsUrl;
        }

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic json = JsonConvert.DeserializeObject(m_WebClient.DownloadString(m_StatsUrl));
            return new CoinNetworkStatistics
            {
                Difficulty = (double)json.nodes[0].difficulty,
                Height = (long)json.nodes[0].height,
                NetHashRate = 0
            };
        }
    }
}
